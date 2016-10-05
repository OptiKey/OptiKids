using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.OptiKids.Extensions;
using JuliusSweetland.OptiKids.Models;
using JuliusSweetland.OptiKids.Services;
using JuliusSweetland.OptiKids.UI.Utilities;
using log4net;

namespace JuliusSweetland.OptiKids.UI.Controls
{
    public class KeyboardHost : ContentControl
    {
        #region Private member vars

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public KeyboardHost()
        {
            Loaded += OnLoaded;

            var contentDp = DependencyPropertyDescriptor.FromProperty(ContentProperty, typeof(KeyboardHost));
            if (contentDp != null)
            {
                contentDp.AddValueChanged(this, ContentChangedHandler);
            }
        }

        #endregion

        #region Properties
        
        public static readonly DependencyProperty LettersProperty =
            DependencyProperty.Register("Letters", typeof (string), typeof (KeyboardHost),
                new PropertyMetadata(default(string),
                    (o, args) =>
                    {
                        var keyboardHost = o as KeyboardHost;
                        if (keyboardHost != null)
                        {
                            keyboardHost.GenerateContent();
                        }
                    }));

        public string Letters
        {
            get { return (string) GetValue(LettersProperty); }
            set { SetValue(LettersProperty, value); }
        }

        public static readonly DependencyProperty PointToKeyValueMapProperty =
            DependencyProperty.Register("PointToKeyValueMap", typeof(Dictionary<Rect, KeyValue>),
                typeof(KeyboardHost), new PropertyMetadata(default(Dictionary<Rect, KeyValue>)));

        public Dictionary<Rect, KeyValue> PointToKeyValueMap
        {
            get { return (Dictionary<Rect, KeyValue>)GetValue(PointToKeyValueMapProperty); }
            set { SetValue(PointToKeyValueMapProperty, value); }
        }

        public static readonly DependencyProperty ErrorContentProperty =
            DependencyProperty.Register("ErrorContent", typeof (object), typeof (KeyboardHost), new PropertyMetadata(default(object)));

        public object ErrorContent
        {
            get { return GetValue(ErrorContentProperty); }
            set { SetValue(ErrorContentProperty, value); }
        }

        #endregion

        #region OnLoaded - build key map

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Log.Debug("KeyboardHost loaded.");

            BuildPointToKeyMap();

            SubscribeToSizeChanges();

            var parentWindow = Window.GetWindow(this);

            if (parentWindow == null)
            {
                var windowException = new ApplicationException(Properties.Resources.PARENT_WINDOW_COULD_NOT_BE_FOUND);

                Log.Error(windowException);

                throw windowException;
            }
            
            SubscribeToParentWindowMoves(parentWindow);

            Loaded -= OnLoaded; //Ensure this logic only runs once
        }

        #endregion

        #region Generate Content

        private void GenerateContent()
        {
            Log.DebugFormat("GenerateContent called. Letters property is '{0}'", Letters);

            //Clear out point to key map
            PointToKeyValueMap = null;

            object newContent = ErrorContent;

            if (Letters != null)
            {
                var rowsOfLetters = Letters.Split('|');
                var grid = new Grid { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                var rows = rowsOfLetters.Length;
                var columns = rowsOfLetters.Max(row => row.Length);
                for (var rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                }
                for (var columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }
                for (var r = 0; r < rowsOfLetters.Length; r++)
                {
                    for (var c = 0; c < rowsOfLetters[r].Length; c++)
                    {
                        var letter = rowsOfLetters[r][c].ToString();
                        var key = new Key
                        {
                            Text = letter,
                            Value = new KeyValue(letter)
                        };
                        Grid.SetRow(key, r);
                        Grid.SetColumn(key, c);
                        grid.Children.Add(key);
                    }
                }
                newContent = grid;
            }

            Content = newContent;
        }

        #endregion
        
        #region Content Change Handler

        private static void ContentChangedHandler(object sender, EventArgs e)
        {
            var keyboardHost = sender as KeyboardHost;
            if (keyboardHost != null)
            {
                keyboardHost.BuildPointToKeyMap();
            }
        }
        
        #endregion

        #region Build Point To Key Map

        private void BuildPointToKeyMap()
        {
            Log.Debug("Building PointToKeyMap.");

            var contentAsFrameworkElement = Content as FrameworkElement;
            if (contentAsFrameworkElement != null)
            {
                if (contentAsFrameworkElement.IsLoaded)
                {
                    TraverseAllKeysAndBuildPointToKeyValueMap();
                }
                else
                {
                    RoutedEventHandler loaded = null;
                    loaded = (sender, args) =>
                    {
                        TraverseAllKeysAndBuildPointToKeyValueMap();
                        contentAsFrameworkElement.Loaded -= loaded;
                    };
                    contentAsFrameworkElement.Loaded += loaded;
                }
            }
        }

        private void TraverseAllKeysAndBuildPointToKeyValueMap()
        {
            var allKeys = VisualAndLogicalTreeHelper.FindVisualChildren<Key>(this).ToList();

            var pointToKeyValueMap = new Dictionary<Rect, KeyValue>();

            var topLeftPoint = new Point(0, 0);

            foreach (var key in allKeys)
            {
                if (key.Value.FunctionKey != null
                    || key.Value.String != null)
                {
                    var rect = new Rect
                    {
                        Location = key.PointToScreen(topLeftPoint),
                        Size = (Size) key.GetTransformToDevice().Transform((Vector) key.RenderSize)
                    };

                    if (rect.Size.Width != 0 && rect.Size.Height != 0)
                    {
                        pointToKeyValueMap.Add(rect, key.Value);
                    }
                }
            }

            PointToKeyValueMap = pointToKeyValueMap;
        }

        #endregion

        #region Subscribe To Size Changes

        private void SubscribeToSizeChanges()
        {
            Observable.FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>
                (h => SizeChanged += h,
                h => SizeChanged -= h)
                .Throttle(TimeSpan.FromSeconds(0.1))
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    Log.Debug("SizeChanged event detected.");

                    BuildPointToKeyMap();
                });
        }

        #endregion

        #region Subscribe To Parent Window Moves

        private void SubscribeToParentWindowMoves(Window parentWindow)
        {
            //This event will also fire if the window is mimised, restored, or maximised, so no need to monitor StateChanged
            Observable.FromEventPattern<EventHandler, EventArgs>
                (h => parentWindow.LocationChanged += h,
                h => parentWindow.LocationChanged -= h)
                .Throttle(TimeSpan.FromSeconds(0.1))
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    Log.Debug("Window's LocationChanged event detected.");
                    BuildPointToKeyMap();
                });
        }

        #endregion
    }
}

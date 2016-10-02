using System;
using System.ComponentModel;
using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Extensions;
using JuliusSweetland.OptiKids.Models;
using JuliusSweetland.OptiKids.Properties;
using JuliusSweetland.OptiKids.UI.Utilities;
using JuliusSweetland.OptiKids.UI.ViewModels;

namespace JuliusSweetland.OptiKids.UI.Controls
{
    public class Key : UserControl, INotifyPropertyChanged
    {
        #region Private Member Vars

        private CompositeDisposable onUnloaded = null;

        #endregion

        #region Ctor

        public Key()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #endregion

        #region On Loaded

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            onUnloaded = new CompositeDisposable();

            var keyboardHost = VisualAndLogicalTreeHelper.FindVisualParent<KeyboardHost>(this);
            var mainViewModel = keyboardHost.DataContext as MainViewModel;
            var keyStateService = mainViewModel.KeyStateService;

            //Calculate SelectionProgress and SelectionInProgress
            var keySelectionProgressSubscription = keyStateService.KeySelectionProgress[Value]
                .OnPropertyChanges(ksp => ksp.Value)
                .Subscribe(value =>
                {
                    SelectionProgress = value;
                    SelectionInProgress = value > 0d;
                });
            onUnloaded.Add(keySelectionProgressSubscription);
            var progress = keyStateService.KeySelectionProgress[Value].Value;
            SelectionProgress = progress;
            SelectionInProgress = progress > 0d;

            //Calculate IsCurrent
            Action<KeyValue?> calculateIsCurrent = value => IsCurrent = value != null && value.Value.Equals(Value);
            var currentPositionSubscription = mainViewModel.OnPropertyChanges(vm => vm.CurrentPositionKey)
                .Subscribe(calculateIsCurrent);
            onUnloaded.Add(currentPositionSubscription);
            calculateIsCurrent(mainViewModel.CurrentPositionKey);

            //Publish own version of CorrectKeySelection event
            var correctKeySelectionSubscription = Observable.FromEventPattern<KeyValue>(
                handler => mainViewModel.CorrectKeySelection += handler,
                handler => mainViewModel.CorrectKeySelection -= handler)
                .Subscribe(pattern =>
                {
                    if (pattern.EventArgs.Equals(Value)
                        && CorrectSelection != null)
                    {
                        CorrectSelection(this, null);
                    }
                });
            onUnloaded.Add(correctKeySelectionSubscription);

            //Publish own version of IncorrectKeySelection event
            var incorrectKeySelectionSubscription = Observable.FromEventPattern<KeyValue>(
                handler => mainViewModel.IncorrectKeySelection += handler,
                handler => mainViewModel.IncorrectKeySelection -= handler)
                .Subscribe(pattern =>
                {
                    if (pattern.EventArgs.Equals(Value)
                        && IncorrectSelection != null)
                    {
                        IncorrectSelection(this, null);
                    }
                });
            onUnloaded.Add(incorrectKeySelectionSubscription);
        }
        
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (onUnloaded != null
                && !onUnloaded.IsDisposed)
            {
                onUnloaded.Dispose();
                onUnloaded = null;
            }
        }

        #endregion

        #region Events

        public event EventHandler CorrectSelection;
        public event EventHandler IncorrectSelection;

        #endregion

        #region Properties

        public static readonly DependencyProperty IsCurrentProperty =
            DependencyProperty.Register("IsCurrent", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool IsCurrent
        {
            get { return (bool) GetValue(IsCurrentProperty); }
            set { SetValue(IsCurrentProperty, value); }
        }

        public static readonly DependencyProperty SelectionProgressProperty =
            DependencyProperty.Register("SelectionProgress", typeof(double), typeof(Key), new PropertyMetadata(default(double)));

        public double SelectionProgress
        {
            get { return (double) GetValue(SelectionProgressProperty); }
            set { SetValue(SelectionProgressProperty, value); }
        }

        public static readonly DependencyProperty SelectionInProgressProperty =
            DependencyProperty.Register("SelectionInProgress", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool SelectionInProgress
        {
            get { return (bool) GetValue(SelectionInProgressProperty); }
            set { SetValue(SelectionInProgressProperty, value); }
        }
        
        //Specify if this key spans multiple keys horizontally - used to keep the contents proportional to other keys
        public static readonly DependencyProperty WidthSpanProperty =
            DependencyProperty.Register("WidthSpan", typeof(double), typeof(Key), new PropertyMetadata(1d));

        public double WidthSpan
        {
            get { return (double) GetValue(WidthSpanProperty); }
            set { SetValue(WidthSpanProperty, value); }
        }

        //Specify if this key spans multiple keys vertically - used to keep the contents proportional to other keys
        public static readonly DependencyProperty HeightSpanProperty =
            DependencyProperty.Register("HeightSpan", typeof(double), typeof(Key), new PropertyMetadata(1d));

        public double HeightSpan
        {
            get { return (double) GetValue(HeightSpanProperty); }
            set { SetValue(HeightSpanProperty, value); }
        }

        public static readonly DependencyProperty SharedSizeGroupProperty =
            DependencyProperty.Register("SharedSizeGroup", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string SharedSizeGroup
        {
            get { return (string) GetValue(SharedSizeGroupProperty); }
            set { SetValue(SharedSizeGroupProperty, value); }
        }
        
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue Value
        {
            get { return (KeyValue)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        #endregion

        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}

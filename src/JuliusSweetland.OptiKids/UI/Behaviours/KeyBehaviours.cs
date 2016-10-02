using System;
using System.Windows;
using System.Windows.Media.Animation;
using JuliusSweetland.OptiKids.UI.Controls;

namespace JuliusSweetland.OptiKids.UI.Behaviours
{
    public static class KeyBehaviours
    {
        #region BeginAnimationOnCorrectKeySelectionEvent

        public static readonly DependencyProperty BeginAnimationOnCorrectKeySelectionEventProperty =
            DependencyProperty.RegisterAttached("BeginAnimationOnCorrectKeySelectionEvent", typeof (Storyboard), typeof (KeyBehaviours),
            new PropertyMetadata(default(Storyboard), BeginAnimationOnCorrectKeySelectionEventChanged));

        private static void BeginAnimationOnCorrectKeySelectionEventChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var storyboard = dependencyPropertyChangedEventArgs.NewValue as Storyboard;
            var frameworkElement = dependencyObject as FrameworkElement;
            var key = frameworkElement.TemplatedParent as Key;
                
            EventHandler selectionHandler = (sender, args) => storyboard.Begin(frameworkElement);
            frameworkElement.Loaded += (sender, args) =>
            {
                if (key != null)
                {
                    key.CorrectSelection += selectionHandler;
                }
            };
            frameworkElement.Unloaded += (sender, args) =>
            {
                if (key != null)
                {
                    key.CorrectSelection -= selectionHandler;
                }
            };
        }

        public static void SetBeginAnimationOnCorrectKeySelectionEvent(DependencyObject element, Storyboard value)
        {
            element.SetValue(BeginAnimationOnCorrectKeySelectionEventProperty, value);
        }

        public static Storyboard GetBeginAnimationOnCorrectKeySelectionEvent(DependencyObject element)
        {
            return (Storyboard)element.GetValue(BeginAnimationOnCorrectKeySelectionEventProperty);
        }

        #endregion

        #region BeginAnimationOnIncorrectKeySelectionEvent

        public static readonly DependencyProperty BeginAnimationOnIncorrectKeySelectionEventProperty =
            DependencyProperty.RegisterAttached("BeginAnimationOnIncorrectKeySelectionEvent", typeof(Storyboard), typeof(KeyBehaviours),
            new PropertyMetadata(default(Storyboard), BeginAnimationOnIncorrectKeySelectionEventChanged));

        private static void BeginAnimationOnIncorrectKeySelectionEventChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var storyboard = dependencyPropertyChangedEventArgs.NewValue as Storyboard;
            var frameworkElement = dependencyObject as FrameworkElement;
            var key = frameworkElement.TemplatedParent as Key;

            EventHandler selectionHandler = (sender, args) => storyboard.Begin(frameworkElement);
            frameworkElement.Loaded += (sender, args) =>
            {
                if (key != null)
                {
                    key.IncorrectSelection += selectionHandler;
                }
            };
            frameworkElement.Unloaded += (sender, args) =>
            {
                if (key != null)
                {
                    key.IncorrectSelection -= selectionHandler;
                }
            };
        }

        public static void SetBeginAnimationOnIncorrectKeySelectionEvent(DependencyObject element, Storyboard value)
        {
            element.SetValue(BeginAnimationOnIncorrectKeySelectionEventProperty, value);
        }

        public static Storyboard GetBeginAnimationOnIncorrectKeySelectionEvent(DependencyObject element)
        {
            return (Storyboard)element.GetValue(BeginAnimationOnIncorrectKeySelectionEventProperty);
        }

        #endregion
    }
}

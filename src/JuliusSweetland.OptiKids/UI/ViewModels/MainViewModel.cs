using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Extensions;
using JuliusSweetland.OptiKids.Models;
using JuliusSweetland.OptiKids.Properties;
using JuliusSweetland.OptiKids.Services;
using log4net;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKids.UI.ViewModels
{
    public partial class MainViewModel : BindableBase
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAudioService audioService;
        private readonly IInputService inputService;
        private readonly IKeyStateService keyStateService;
        private readonly Dictionary<char, string> pronunciation;
        private readonly Quiz quiz;

        private KeyValue? currentPositionKey;
        private Dictionary<Rect, KeyValue> pointToKeyValueMap;

        #endregion

        #region Ctor

        public MainViewModel(IAudioService audioService, IInputService inputService, 
            IKeyStateService keyStateService, Dictionary<char, string> pronunciation, Quiz quiz)
        {
            this.audioService = audioService;
            this.inputService = inputService;
            this.keyStateService = keyStateService;
            this.pronunciation = pronunciation;
            this.quiz = quiz;
        }

        #endregion

        #region Events

        public event EventHandler<NotificationEventArgs> ToastNotification;
        public event EventHandler<KeyValue> CorrectKeySelection;
        public event EventHandler<KeyValue> IncorrectKeySelection;

        #endregion

        #region Properties

        public IInputService InputService { get { return inputService; } }
        public IKeyStateService KeyStateService { get { return keyStateService; } }

        public Dictionary<Rect, KeyValue> PointToKeyValueMap
        {
            set
            {
                if (pointToKeyValueMap != value)
                {
                    pointToKeyValueMap = value;
                    inputService.PointToKeyValueMap = value;
                }
            }
        }

        public KeyValue? CurrentPositionKey
        {
            get { return currentPositionKey; }
            set { SetProperty(ref currentPositionKey, value); }
        }

        #endregion

        #region Methods

        public void StartQuiz()
        {
            //TODO: Run the quiz
        }

        public void FireCorrectKeySelectionEvent(KeyValue kv)
        {
            if (CorrectKeySelection != null)
            {
                CorrectKeySelection(this, kv);
            }
        }

        public void FireIncorrectKeySelectionEvent(KeyValue kv)
        {
            if (IncorrectKeySelection != null)
            {
                IncorrectKeySelection(this, kv);
            }
        }

        private void ResetSelectionProgress()
        {
            if (keyStateService != null)
            {
                keyStateService.KeySelectionProgress.Clear();
            }
        }

        internal void RaiseToastNotification(string title, string content, NotificationTypes notificationType, Action callback)
        {
            if (ToastNotification != null)
            {
                ToastNotification(this, new NotificationEventArgs(title, content, notificationType, callback));
            }
        }

        #endregion
    }
}

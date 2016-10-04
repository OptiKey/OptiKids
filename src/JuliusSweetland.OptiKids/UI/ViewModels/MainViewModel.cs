using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Models;
using JuliusSweetland.OptiKids.Properties;
using JuliusSweetland.OptiKids.Services;
using log4net;
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

        private List<Question> questions;
        private int questionIndex = 0;
        private string imagePath;
        private string word;
        private string wordProgress;
        private List<string> letters;
        private int letterIndex = 0;
        private int letterGuessCount = 0;

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

        public string ImagePath
        {
            get { return imagePath; }
            set { SetProperty(ref imagePath, value); }
        }

        private List<string> Letters
        {
            get { return letters; }
            set { SetProperty(ref letters, value); }
        }

        private string WordProgress
        {
            get { return wordProgress; }
            set { SetProperty(ref wordProgress, value); }
        }

        #endregion

        #region Methods

        public void StartQuiz()
        {
            questions = quiz.Questions;
            if (quiz.RandomiseWords)
            {
                var rnd = new Random();
                questions = questions.OrderBy(x => rnd.Next()).ToList();
            }
            questionIndex = 0;
            SetQuestion();
        }

        private async void SetQuestion()
        {
            var question = questions.Count > questionIndex
                ? questions[questionIndex]
                : null;

            if (question != null)
            {
                InputService.RequestSuspend();
                ImagePath = question.ImagePath;
                audioService.SpeakNewOrInterruptCurrentSpeech("Test", null);
                await Task.Delay(quiz.DisplayImageForXSeconds * 1000);
                ImagePath = null;

                word = questions[questionIndex].Word;
                WordProgress = quiz.DisplayWordMasks
                    ? word.Select(c => '_').ToString()
                    : null;

                var orderedLetters = questions[questionIndex].Letters.Select(c => c.ToString()).ToList();
                if (quiz.RandomiseLetters)
                {
                    var rnd = new Random();
                    orderedLetters = orderedLetters.OrderBy(x => rnd.Next()).ToList();
                }
                Letters = orderedLetters;
            }
            else
            {
                FinishQuiz();
            }
        }

        private void Speak(string word)
        {
            audioService.SpeakNewOrInterruptCurrentSpeech(word, null, null, Settings.Default.WordSpeechRate);

            var phonemes = ;

            if (string.IsNullOrEmpty(phonemes))
            {
                foreach (var letter in word.Where(c => Char.IsLetter(c)).ToList())
                {
                    audioService.SpeakNewOrInterruptCurrentSpeech(letter.ToString(), null, null, Settings.Default.SpellingSpeechRate);
                }
            }
            else
            {
                foreach (var phoneme in phonemes.Split('|'))
                {
                    var pb = new PromptBuilder();
                    string ssml = string.Format("<phoneme alphabet=\"x-microsoft-ups\" ph=\"{0}\">{0}</phoneme>", phoneme);
                    pb.AppendSsmlMarkup(ssml);
                    synth.Speak(pb);
                }
            }
        }

        private void ProgressQuestion()
        {
            questionIndex++;
            letterIndex = 0;
            letterGuessCount = 0;
        }

        private void FinishQuiz()
        {
            //TODO: Finish quiz
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

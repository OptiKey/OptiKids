using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
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
        private int wordIndex = 0;
        private string wordProgress;
        private string letters;
        private int guessCount = 0;

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

        public string Letters
        {
            get { return letters; }
            set { SetProperty(ref letters, value); }
        }

        public string WordProgress
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
                word = questions[questionIndex].Word.Trim();
                WordProgress = new string(word.Select(c => quiz.DisplayWordMasks ? '_' : ' ').ToArray());

                var orderedLetters = questions[questionIndex].Letters;
                if (quiz.RandomiseLetters)
                {
                    var rnd = new Random();
                    var randomisedLetters = orderedLetters.Where(c => c != '|').OrderBy(x => rnd.Next()).ToList();
                    for(var index = 0; index < orderedLetters.Length; index++)
                    {
                        if (orderedLetters[index] == '|')
                        {
                            randomisedLetters.Insert(index, '|');
                        }
                    }
                    orderedLetters = string.Join("", randomisedLetters);
                }
                Letters = orderedLetters;

                InputService.RequestSuspend();
                ImagePath = question.ImagePath;
                var minImageDisplayTime = Task.Delay(Settings.Default.MinImageDisplayTimeInSeconds * 1000);
                var speakTask = Speak(word, quiz.SpellingAudioHints);
                await Task.WhenAll(minImageDisplayTime, speakTask);
                ImagePath = null;
                InputService.RequestResume();
            }
            else
            {
                FinishQuiz();
            }
        }

        private async Task Speak(string text, bool spell)
        {
            var wordTcs = new TaskCompletionSource<bool>(); //Used to make this method awaitable
            audioService.SpeakNewOrInterruptCurrentSpeech(text, 
                () => wordTcs.SetResult(true), null, Settings.Default.WordSpeechRate);
            await wordTcs.Task;

            if (spell)
            {
                await Spell(text);
            }
        }

        private async Task Spell(string word)
        {
            var ssmls = word.Where(Char.IsLetter)
                .Select(c => pronunciation.ContainsKey(c)
                    ? string.Format("<phoneme alphabet=\"x-microsoft-ups\" ph=\"{0}\">{0}</phoneme><break />", pronunciation[c])
                    : string.Format("{0}<break />", c));
            var promptBuilder = new PromptBuilder();
            foreach (var ssml in ssmls)
            {
                promptBuilder.AppendSsmlMarkup(ssml);
            }
            var spellingTcs = new TaskCompletionSource<bool>();
            audioService.SpeakNewOrInterruptCurrentSpeech(promptBuilder,
                () => spellingTcs.SetResult(true), null, Settings.Default.SpellingSpeechRate);
            await spellingTcs.Task;
        }

        private void ProgressQuestion()
        {
            questionIndex++;
            if (questionIndex == questions.Count)
            {
                FinishQuiz();
            }
            else
            {
                wordIndex = 0;
                guessCount = 0;
                SetQuestion();
            }
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

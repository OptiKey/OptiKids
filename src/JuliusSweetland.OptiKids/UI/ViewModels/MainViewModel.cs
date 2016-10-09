using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Extensions;
using JuliusSweetland.OptiKids.Models;
using JuliusSweetland.OptiKids.Properties;
using JuliusSweetland.OptiKids.Services;
using log4net;
using Newtonsoft.Json;
using Prism.Commands;
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
        private readonly List<INotifyErrors> errorNotifyingServices;
        private readonly Dictionary<char, string> pronunciation;
        private Quiz quiz;
        
        private KeyValue? currentPositionKey;
        private Dictionary<Rect, KeyValue> pointToKeyValueMap;

        private List<Question> questions;
        private int questionIndex = 0;
        private string imagePath;
        private string word;
        private int wordIndex = 0;
        private string wordProgress;
        private string letters;
        private int incorrectGuessCount = 0;
        private QuizStates quizState = QuizStates.WaitingToStart;

        #endregion

        #region Ctor

        public MainViewModel(IAudioService audioService, IInputService inputService, 
            IKeyStateService keyStateService, List<INotifyErrors> errorNotifyingServices, 
            Dictionary<char, string> pronunciation)
        {
            this.audioService = audioService;
            this.inputService = inputService;
            this.keyStateService = keyStateService;
            this.errorNotifyingServices = errorNotifyingServices;
            this.pronunciation = pronunciation;

            Settings.Default.OnPropertyChanges(s => s.QuizFile).Subscribe(_ => OnPropertyChanged(() => QuizFileName));

            StartQuizCommand = new DelegateCommand(StartQuiz);
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

        public QuizStates QuizState
        {
            get { return quizState; }
            set { SetProperty(ref quizState, value); }
        }

        public string QuizFileName
        {
            get { return Path.GetFileName(Settings.Default.QuizFile); }
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

        public DelegateCommand StartQuizCommand { get; private set; }

        #endregion

        #region Methods

        public void StartQuiz()
        {
            quiz = null;
            try
            {
                var quizString = File.ReadAllText(Settings.Default.QuizFile);
                quiz = JsonConvert.DeserializeObject<Quiz>(quizString);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unable to load and deserialise the quiz file. Exception message:{0}\nStackTrace:{1}", ex.Message, ex.StackTrace);
                RaiseToastNotification(Resources.CANNOT_START_QUIZ_TITLE, Resources.CANNOT_START_QUIZ_CONTENT, NotificationTypes.Error, null);
                return;
            }

            QuizState = QuizStates.Running;
            questions = quiz.Questions;
            if (quiz.RandomiseWords)
            {
                var rnd = new Random();
                questions = questions.OrderBy(x => rnd.Next()).ToList();
            }
            questionIndex = 0;
            RaiseToastNotification(Resources.START_QUIZ_TITLE, Resources.START_QUIZ_CONTENT, NotificationTypes.Normal, SetQuestion);
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

        private async Task Speak(string text, bool spell, int? overrideSpeakRate = null)
        {
            var wordTcs = new TaskCompletionSource<bool>(); //Used to make this method awaitable
            audioService.SpeakNewOrInterruptCurrentSpeech(text, 
                () => wordTcs.SetResult(true), null, 
                overrideSpeakRate != null ? overrideSpeakRate.Value : Settings.Default.WordSpeechRate);
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
            wordIndex = 0;
            incorrectGuessCount = 0;
            SetQuestion();
        }

        private void FinishQuiz()
        {
            QuizState = QuizStates.Finished;
            RaiseToastNotification(Resources.QUIZ_COMPLETE_TITLE, Resources.QUIZ_COMPLETE_CONTENT, 
                NotificationTypes.Normal, () => QuizState = QuizStates.WaitingToStart);
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

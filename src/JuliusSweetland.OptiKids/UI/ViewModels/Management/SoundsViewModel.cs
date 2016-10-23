using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JuliusSweetland.OptiKids.Extensions;
using JuliusSweetland.OptiKids.Properties;
using JuliusSweetland.OptiKids.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKids.UI.ViewModels.Management
{
    public class SoundsViewModel : BindableBase
    {
        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IAudioService audioService;

        #endregion
        
        #region Ctor

        public SoundsViewModel(IAudioService audioService)
        {
            this.audioService = audioService;

            InfoSoundPlayCommand = new DelegateCommand(() => audioService.PlaySound(InfoSoundFile, InfoSoundVolume));
            ErrorSoundPlayCommand = new DelegateCommand(() => audioService.PlaySound(ErrorSoundFile, ErrorSoundVolume));

            this.OnPropertyChanges(s => s.PronunciationFile).Subscribe(_ => OnPropertyChanged(() => PronunciationFileName));

            Load();
        }
        
        #endregion
        
        #region Properties

        public List<KeyValuePair<string, string>> SpeechVoices
        {
            get { return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(Resources.DEFAULT, null)
            }.Concat(audioService.GetAvailableVoices().Select(v => new KeyValuePair<string, string>(v, v))).ToList(); }
        }
        
        public List<KeyValuePair<string, string>> GeneralSoundFiles
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(Resources.NO_SOUND, null),
                    new KeyValuePair<string, string>(Resources.TONE_SOUND_1, @"Resources\Sounds\Tone1.wav"),
                    new KeyValuePair<string, string>(Resources.TONE_SOUND_2, @"Resources\Sounds\Tone2.wav"),
                    new KeyValuePair<string, string>(Resources.TONE_SOUND_3, @"Resources\Sounds\Tone3.wav"),
                    new KeyValuePair<string, string>(Resources.TONE_SOUND_4, @"Resources\Sounds\Tone4.wav")
                };
            }
        }

        private string speechVoice;
        public string SpeechVoice
        {
            get { return speechVoice; }
            set { SetProperty(ref speechVoice, value); }
        }
        
        private int speechVolume;
        public int SpeechVolume
        {
            get { return speechVolume; }
            set { SetProperty(ref speechVolume, value); }
        }

        private int speechRate;
        public int SpeechRate
        {
            get { return speechRate; }
            set { SetProperty(ref speechRate, value); }
        }

        private int wordSpeechRate;
        public int WordSpeechRate
        {
            get { return wordSpeechRate; }
            set { SetProperty(ref wordSpeechRate, value); }
        }

        private int spellingSpeechRate;
        public int SpellingSpeechRate
        {
            get { return spellingSpeechRate; }
            set { SetProperty(ref spellingSpeechRate, value); }
        }

        private string pronunciationFile;
        public string PronunciationFile
        {
            get { return pronunciationFile; }
            set { SetProperty(ref pronunciationFile, value); }
        }

        public string PronunciationFileName
        {
            get { return Path.GetFileName(PronunciationFile); }
        }

        private bool playEncouragementOnCorrectlySpelledWord;
        public bool PlayEncouragementOnCorrectlySpelledWord
        {
            get { return playEncouragementOnCorrectlySpelledWord; }
            set { SetProperty(ref playEncouragementOnCorrectlySpelledWord, value); }
        }

        private string infoSoundFile;
        public string InfoSoundFile
        {
            get { return infoSoundFile; }
            set { SetProperty(ref infoSoundFile, value); }
        }

        private int infoSoundVolume;
        public int InfoSoundVolume
        {
            get { return infoSoundVolume; }
            set { SetProperty(ref infoSoundVolume, value); }
        }

        private string errorSoundFile;
        public string ErrorSoundFile
        {
            get { return errorSoundFile; }
            set { SetProperty(ref errorSoundFile, value); }
        }

        private int errorSoundVolume;
        public int ErrorSoundVolume
        {
            get { return errorSoundVolume; }
            set { SetProperty(ref errorSoundVolume, value); }
        }

        public bool ChangesRequireRestart
        {
            get { return false; }
        }
        
        public DelegateCommand InfoSoundPlayCommand { get; private set; }
        public DelegateCommand ErrorSoundPlayCommand { get; private set; }
        
        #endregion
        
        #region Methods

        private void Load()
        {
            SpeechVoice = Settings.Default.SpeechVoice;
            SpeechVolume = Settings.Default.SpeechVolume;
            SpeechRate = Settings.Default.SpeechRate;
            WordSpeechRate = Settings.Default.WordSpeechRate;
            SpellingSpeechRate = Settings.Default.SpellingSpeechRate;
            PronunciationFile = Settings.Default.PronunciationFile;
            PlayEncouragementOnCorrectlySpelledWord = Settings.Default.PlayEncouragementOnCorrectlySpelledWord;
            InfoSoundFile = Settings.Default.InfoSoundFile;
            InfoSoundVolume = Settings.Default.InfoSoundVolume;
            ErrorSoundFile = Settings.Default.ErrorSoundFile;
            ErrorSoundVolume = Settings.Default.ErrorSoundVolume;
        }

        public void ApplyChanges()
        {
            Settings.Default.SpeechVoice = SpeechVoice;
            Settings.Default.SpeechVolume = SpeechVolume;
            Settings.Default.SpeechRate = SpeechRate;
            Settings.Default.WordSpeechRate = WordSpeechRate;
            Settings.Default.SpellingSpeechRate = SpellingSpeechRate;
            Settings.Default.PronunciationFile = PronunciationFile;
            Settings.Default.PlayEncouragementOnCorrectlySpelledWord = PlayEncouragementOnCorrectlySpelledWord;
            Settings.Default.InfoSoundFile = InfoSoundFile;
            Settings.Default.InfoSoundVolume = InfoSoundVolume;
            Settings.Default.ErrorSoundFile = ErrorSoundFile;
            Settings.Default.ErrorSoundVolume = ErrorSoundVolume;
        }

        #endregion
    }
}

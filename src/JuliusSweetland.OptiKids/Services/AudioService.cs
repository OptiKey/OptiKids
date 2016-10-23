using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using JuliusSweetland.OptiKids.Properties;
using log4net;
using Un4seen.Bass;

namespace JuliusSweetland.OptiKids.Services
{
    public class AudioService : IAudioService
    {
        #region Constants

        private const string BassRegistrationEmail = "optikeyfeedback@gmail.com";
        private const string BassRegistrationKey = "2X24252025152222";

        #endregion

        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly SpeechSynthesizer speechSynthesiser;

        private EventHandler<SpeakCompletedEventArgs> speakCompleted;
        
        #endregion

        #region Events

        public event EventHandler<Exception> Error;

        #endregion

        #region Ctor

        public AudioService()
        {
            speechSynthesiser = new SpeechSynthesizer();
            BassNet.Registration(BassRegistrationEmail, BassRegistrationKey);
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            Application.Current.Exit += (sender, args) => Bass.BASS_Free();
        }

        #endregion

        #region Public methods
        
        /// <summary>
        /// Start speaking the supplied text
        /// </summary>
        public void Speak(string textToSpeak, Action onComplete, int? volume = null, int? rate = null, string voice = null)
        {
            Log.Info("Speak called");

            try
            {
                if (string.IsNullOrEmpty(textToSpeak)) return;

                speechSynthesiser.Rate = rate ?? Settings.Default.SpeechRate;
                speechSynthesiser.Volume = volume ?? Settings.Default.SpeechVolume;

                var voiceToUse = voice ?? Settings.Default.SpeechVoice;
                if (!string.IsNullOrWhiteSpace(voiceToUse))
                {
                    try
                    {
                        speechSynthesiser.SelectVoice(voiceToUse);
                    }
                    catch (Exception exception)
                    {
                        var customException = new ApplicationException(string.Format(Resources.UNABLE_TO_SET_VOICE_WARNING,
                            voiceToUse, voice == null ? Resources.VOICE_COMES_FROM_SETTINGS : null), exception);
                        PublishError(this, customException);
                    }
                }

                speakCompleted = (sender, args) =>
                {
                    speechSynthesiser.SpeakCompleted -= speakCompleted;
                    speakCompleted = null;
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                };
                speechSynthesiser.SpeakCompleted += speakCompleted;

                Log.InfoFormat("Speaking '{0}' with volume '{1}', rate '{2}' and voice '{3}'", textToSpeak, volume, rate, voice);
                speechSynthesiser.SpeakAsync(textToSpeak);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        /// <summary>
        /// Start speaking the supplied text, or cancel the in-progress speech
        /// </summary>
        public void SpeakSsml(IEnumerable<string> ssmls, Action onComplete, int? volume = null, int? rate = null, string voice = null)
        {
            Log.Info("SpeakSsml called");

            try
            {
                if (!ssmls.Any()) return;

                speechSynthesiser.Rate = rate ?? Settings.Default.SpeechRate;
                speechSynthesiser.Volume = volume ?? Settings.Default.SpeechVolume;
                
                speakCompleted = (sender, args) =>
                {
                    if (speakCompleted != null)
                    {
                        speechSynthesiser.SpeakCompleted -= speakCompleted;
                    }
                    speakCompleted = null;
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                };
                speechSynthesiser.SpeakCompleted += speakCompleted;

                var pb = new PromptBuilder();
                string voiceToUse = voice ?? Settings.Default.SpeechVoice;
                if (!string.IsNullOrWhiteSpace(voiceToUse))
                {
                    pb.AppendSsmlMarkup(string.Format("<voice name=\"{0}\">", voiceToUse));
                }
                foreach (var ssml in ssmls)
                {
                    pb.AppendSsmlMarkup(ssml);
                }
                if (!string.IsNullOrWhiteSpace(voiceToUse))
                {
                    pb.AppendSsmlMarkup("</voice>");
                }
                Log.InfoFormat("Speaking prompt '{0}' with volume '{1}', rate '{2}' and voice '{3}'", pb.ToXml(), volume, rate, voice);
                speechSynthesiser.SpeakAsync(pb);
            }
            catch (Exception exception)
            {
                PublishError(this, exception);
            }
        }

        public List<string> GetAvailableVoices()
        {
            Log.Info("GetAvailableVoices called");
            var availableVoices = new SpeechSynthesizer().GetInstalledVoices()
                .Where(v => v.Enabled)
                .Select(v => v.VoiceInfo.Name)
                .ToList();

            Log.InfoFormat("GetAvailableVoices returing {0} voices", availableVoices.Count);

            return availableVoices;
        }

        public void PlaySound(string file, int volume)
        {
            Log.InfoFormat("Playing sound '{0}' at volume '{1}'", file, volume);
            if (string.IsNullOrEmpty(file)) return;

            try
            {
                // create a stream channel from a file 
                int stream = Bass.BASS_StreamCreateFile(file, 0L, 0L, BASSFlag.BASS_DEFAULT | BASSFlag.BASS_STREAM_AUTOFREE);
                if (stream != 0)
                {
                    Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, (volume/100f));
                    Bass.BASS_ChannelPlay(stream, false);
                }
                else
                {
                    throw new ApplicationException(string.Format(Resources.BASS_UNABLE_TO_CREATE_STREAM_FROM_FILE, file));
                }
            }
            catch (Exception exception)
            {
                //Don't publish error - the error handler tries to play a sound file which could loop us right back here
                Log.Error("Exception encountered within the AudioService", exception);
            }
        }

        private void PublishError(object sender, Exception ex)
        {
            Log.Error("Publishing Error event (if there are any listeners)", ex);
            if (Error != null)
            {
                Error(sender, ex);
            }
        }

        #endregion
    }
}

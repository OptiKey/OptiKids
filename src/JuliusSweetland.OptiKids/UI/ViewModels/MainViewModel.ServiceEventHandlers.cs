using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Models;
using JuliusSweetland.OptiKids.Properties;

namespace JuliusSweetland.OptiKids.UI.ViewModels
{
    public partial class MainViewModel
    {
        public void AttachServiceEventHandlers()
        {
            Log.Info("AttachServiceEventHandlers called.");

            if (errorNotifyingServices != null)
            {
                errorNotifyingServices.ForEach(s => s.Error += HandleServiceError);
            }

            inputService.CurrentPosition += (o, tuple) =>
            {
                CurrentPositionKey = tuple.Item2;
            };

            inputService.SelectionProgress += (o, progress) =>
            {
                if (progress.Item1 == null
                    && progress.Item2 == 0)
                {
                    ResetSelectionProgress(); //Reset all keys
                }
                else if (progress.Item1 != null
                    && progress.Item1.Value.KeyValue != null)
                {
                    keyStateService.KeySelectionProgress[progress.Item1.Value.KeyValue.Value] =
                        new NotifyingProxy<double>(progress.Item2);
                }
            };

            inputService.Selection += async (o, value) =>
            {
                Log.InfoFormat("Selection event received from InputService with KeyValue: {0}", value.KeyValue);

                if (value.KeyValue != null
                    && value.KeyValue.Value != null
                    && !string.IsNullOrEmpty(value.KeyValue.Value.String))
                {
                    InputService.RequestSuspend();

                    if (word[wordIndex].ToString() == value.KeyValue.Value.String)
                    {
                        //Correct selection
                        if(CorrectKeySelection != null)
                        {
                            Log.InfoFormat("Firing CorrectKeySelection event with KeyValue '{0}'", value.KeyValue.Value);
                            CorrectKeySelection(this, value.KeyValue.Value);
                        }

                        var newWordProgress = new StringBuilder(wordProgress);
                        newWordProgress.Remove(wordIndex, 1);
                        newWordProgress.Insert(wordIndex, value.KeyValue.Value.String);
                        WordProgress = newWordProgress.ToString();
                        wordIndex++;

                        if (WordProgress == word)
                        {
                            //Word complete
                            CurrentPositionKey = null; //Clear current position key - it is distracting
                            await Spell(value.KeyValue.Value.String);
                            if (Settings.Default.PlayEncouragementOnCorrectlySpelledWord)
                            {
                                var rnd = new Random();
                                var encouragement =
                                    Resources.CORRECT_SPELLING_ENCOURAGEMENTS.Split('|')
                                        .OrderBy(x => rnd.Next())
                                        .First();
                                await Speak(encouragement, false, 0);
                            }
                            await Task.Delay(Settings.Default.MinDelayBeforeProgressingInSeconds * 1000);
                            ProgressQuestion();
                        }
                        else
                        {
                            Spell(value.KeyValue.Value.String);
                            incorrectGuessCount = 0;
                        }
                    }
                    else
                    {
                        //Incorrect selection
                        if (IncorrectKeySelection != null)
                        {
                            Log.InfoFormat("Firing IncorrectKeySelection event with KeyValue '{0}'", value.KeyValue.Value);
                            IncorrectKeySelection(this, value.KeyValue.Value);
                        }

                        incorrectGuessCount++;
                        if (quiz.HintEveryXIncorrectLetters > 0 
                            && incorrectGuessCount % quiz.HintEveryXIncorrectLetters == 0)
                        {
                            await Spell(value.KeyValue.Value.String);
                            CurrentPositionKey = null; //Clear current position key - it is distracting
                            await Task.Delay(500); //Slight pause before hint
                            await Speak(word, true);
                        }
                        else
                        {
                            Spell(value.KeyValue.Value.String);
                        }
                    }

                    InputService.RequestResume();
                }
            };

            inputService.PointToKeyValueMap = pointToKeyValueMap;
        }

        private void HandleServiceError(object sender, Exception exception)
        {
            Log.Error("Error event received from service. Raising ErrorNotificationRequest and playing ErrorSoundFile (from settings)", exception);

            inputService.RequestSuspend();
            audioService.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume);
            RaiseToastNotification(Resources.CRASH_TITLE, exception.Message, NotificationTypes.Error, () => inputService.RequestResume());
        }
    }
}

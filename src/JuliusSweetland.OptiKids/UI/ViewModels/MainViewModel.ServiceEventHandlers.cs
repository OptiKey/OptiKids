using System.Text;
using System.Threading.Tasks;
using JuliusSweetland.OptiKids.Models;
using JuliusSweetland.OptiKids.Properties;

namespace JuliusSweetland.OptiKids.UI.ViewModels
{
    public partial class MainViewModel
    {
        public void AttachServiceEventHandlers()
        {
            Log.Info("AttachServiceEventHandlers called.");

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
                            var newWordProgress = new StringBuilder(wordProgress);
                            newWordProgress.Remove(wordIndex, 1);
                            newWordProgress.Insert(wordIndex, value.KeyValue.Value.String);
                            WordProgress = newWordProgress.ToString();
                            wordIndex++;
                            await Spell(value.KeyValue.Value.String);
                            if (WordProgress == word)
                            {
                                //Word complete
                                var minDelayBeforeProgressing = Task.Delay(Settings.Default.MinDelayBeforeProgressingInSeconds * 1000);
                                var speakTask = Speak(word, false);
                                await Task.WhenAll(minDelayBeforeProgressing, speakTask);
                                ProgressQuestion();
                            }
                        }
                    }
                    else
                    {
                        //Incorrect selection
                        if (IncorrectKeySelection != null)
                        {
                            Log.InfoFormat("Firing IncorrectKeySelection event with KeyValue '{0}'", value.KeyValue.Value);
                            IncorrectKeySelection(this, value.KeyValue.Value);
                            await Spell(value.KeyValue.Value.String);
                        }
                    }

                    InputService.RequestResume();
                }
            };

            inputService.PointToKeyValueMap = pointToKeyValueMap;
        }
    }
}

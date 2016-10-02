using JuliusSweetland.OptiKids.Models;

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

            inputService.Selection += (o, value) =>
            {
                Log.Info("Selection event received from InputService.");

                if (value.KeyValue != null)
                {
                    //TODO: Fire CorrectKeySelection / IncorrectKeySelection and play sound?
                    //audioService.PlaySound(Settings.Default.KeySelectionSoundFile, Settings.Default.KeySelectionSoundVolume);
                    //if (IncorrectKeySelection != null)
                    //{
                    //    Log.InfoFormat("Firing IncorrectKeySelection event with KeyValue '{0}'", value.KeyValue.Value);
                    //    IncorrectKeySelection(this, value.KeyValue.Value);
                    //}
                }
            };

            inputService.PointToKeyValueMap = pointToKeyValueMap;
        }
    }
}

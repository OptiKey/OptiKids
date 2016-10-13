using JuliusSweetland.OptiKids.Services;
using Prism.Interactivity.InteractionRequest;

namespace JuliusSweetland.OptiKids.Models
{
    public class NotificationWithAudioService : INotification
    {
        public IAudioService AudioService { get; set; }

        #region INotification

        public string Title { get; set; }
        public object Content { get; set; }

        #endregion
    }
}

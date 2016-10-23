using System;
using System.Collections.Generic;

namespace JuliusSweetland.OptiKids.Services
{
    public interface IAudioService : INotifyErrors
    {
        List<string> GetAvailableVoices();
        void PlaySound(string file, int volume);
        void Speak(string textToSpeak, Action onComplete, int? volume = null, int? rate = null, string voice = null);
        void SpeakSsml(IEnumerable<string> ssmls, Action onComplete, int? volume = null, int? rate = null, string voice = null);
    }
}

using System.ComponentModel;
using JuliusSweetland.OptiKids.Models;

namespace JuliusSweetland.OptiKids.Services
{
    public interface IKeyStateService : INotifyPropertyChanged
    {
        NotifyingConcurrentDictionary<KeyValue, double> KeySelectionProgress { get; }
    }
}

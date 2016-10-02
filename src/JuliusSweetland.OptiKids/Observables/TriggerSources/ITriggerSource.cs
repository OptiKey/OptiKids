using System;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Models;

namespace JuliusSweetland.OptiKids.Observables.TriggerSources
{
    public interface ITriggerSource
    {
        RunningStates State { get; set; }
        IObservable<TriggerSignal> Sequence { get; }
    }
}

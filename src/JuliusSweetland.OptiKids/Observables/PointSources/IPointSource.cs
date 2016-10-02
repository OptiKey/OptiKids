using System;
using System.Collections.Generic;
using System.Reactive;
using System.Windows;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Models;

namespace JuliusSweetland.OptiKids.Observables.PointSources
{
    public interface IPointSource
    {
        RunningStates State { get; set; }
        Dictionary<Rect, KeyValue> PointToKeyValueMap { set; }
        IObservable<Timestamped<PointAndKeyValue?>> Sequence { get; }
    }
}

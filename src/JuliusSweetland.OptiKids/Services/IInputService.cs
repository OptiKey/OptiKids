using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Models;

namespace JuliusSweetland.OptiKids.Services
{
    public interface IInputService : INotifyPropertyChanged, INotifyErrors
    {
        event EventHandler<Tuple<Point, KeyValue?>> CurrentPosition;
        event EventHandler<Tuple<PointAndKeyValue?, double>> SelectionProgress;
        event EventHandler<PointAndKeyValue> Selection;

        Dictionary<Rect, KeyValue> PointToKeyValueMap { set; }

        void RequestSuspend();
        void RequestResume();
    }
}

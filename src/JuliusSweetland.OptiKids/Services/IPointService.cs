using System;
using System.Reactive;
using System.Windows;

namespace JuliusSweetland.OptiKids.Services
{
    public interface IPointService : INotifyErrors
    {
        event EventHandler<Timestamped<Point>> Point;
    }
}

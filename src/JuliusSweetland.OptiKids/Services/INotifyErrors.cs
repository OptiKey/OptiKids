using System;

namespace JuliusSweetland.OptiKids.Services
{
    public interface INotifyErrors
    {
        event EventHandler<Exception> Error;
    }
}

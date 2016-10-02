using Prism.Mvvm;

namespace JuliusSweetland.OptiKids.Models
{
    public class NotifyingProxy<T> : BindableBase
    {
        public NotifyingProxy(T value)
        {
            this.value = value;
        }

        private T value;
        public T Value
        {
            get { return value; }
            set { SetProperty(ref this.value, value); }
        }
    }
}

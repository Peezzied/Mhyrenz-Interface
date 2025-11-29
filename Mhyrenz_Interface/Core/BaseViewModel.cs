using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mhyrenz_Interface
{
    public delegate TViewModel CreateViewModel<out TViewModel>(object parameter = null);

    public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void Dispose() { }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

    }
}

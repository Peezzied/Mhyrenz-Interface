using System;

namespace Mhyrenz_Interface.ViewModels.Factory
{
    public interface IViewModelFactory<T>
    {
        T CreateViewModel(object parameter = null);
    }
}
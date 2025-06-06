using System;

namespace Mhyrenz_Interface.ViewModels.Factory
{
    public interface IViewModelFactory
    {
        BaseViewModel CreateViewModel(Type viewType);
    }
}
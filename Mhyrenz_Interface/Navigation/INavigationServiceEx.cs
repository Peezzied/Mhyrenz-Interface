using System;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Navigation
{
    public interface INavigationServiceEx
    {
        NavigationViewModel CurrentViewModel { get; }
        event Action<NavigationViewModel> CurrentViewModelChanged;

        Task<bool> NavigateAsync(Type viewType, Action<NavigationViewModel> postNavigationCallback = null);
    }
}
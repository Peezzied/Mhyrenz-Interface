using System;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Navigation
{
    public delegate Task PostNavigation(NavigationViewModel navigationViewModel);

    public interface INavigationServiceEx
    {
        NavigationViewModel CurrentViewModel { get; }
        event Action<NavigationViewModel> CurrentViewModelChanged;

        Task<bool> NavigateAsync(Type viewType, PostNavigation postNavigationCallback = null);
    }
}
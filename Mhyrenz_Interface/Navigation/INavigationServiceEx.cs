using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.MVVM;

namespace Mhyrenz_Interface.Navigation
{
    public delegate Task PostNavigation(BaseViewModel navigationViewModel);

    public interface INavigationServiceEx
    {
        BaseViewModel CurrentViewModel { get; }
        event Action<BaseViewModel, Type> Navigated;

        Task<bool> NavigateAsync(Type viewType, PostNavigation postNavigationCallback = null);
    }
}
using Mhyrenz_Interface.ViewModels.Factory;
using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Mhyrenz_Interface.Navigation
{
    public interface INavigationServiceEx
    {
        NavigationViewModel CurrentViewModel { get; set; }

        event EventHandler Navigating;
        event NavigatedEventHandler Navigated;
        event NavigationFailedEventHandler NavigationFailed;
        event Action TransitionCompleted;

        Frame Frame { get; set; }

        bool CanGoBack { get; }
        bool CanGoForward { get; }

        void GoBack();
        void GoForward();

        bool Navigate(Uri sourcePageUri, object extraData = null);
        bool Navigate(Type sourceType);
        void TransitionComplete();
    }
}
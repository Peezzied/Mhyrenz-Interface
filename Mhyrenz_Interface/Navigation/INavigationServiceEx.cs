using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Mhyrenz_Interface.Navigation
{
    public interface INavigationServiceEx
    {
        BaseViewModel CurrentViewModel { get; set; }

        event NavigatedEventHandler Navigated;
        event NavigationFailedEventHandler NavigationFailed;

        Frame Frame { get; set; }

        bool CanGoBack { get; }
        bool CanGoForward { get; }

        void GoBack();
        void GoForward();

        bool Navigate(Uri sourcePageUri, object extraData = null);
        bool Navigate(Type sourceType);
    }
}
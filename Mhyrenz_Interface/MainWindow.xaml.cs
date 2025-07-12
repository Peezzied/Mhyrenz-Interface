using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly INavigationServiceEx _navigationServiceEx;
        public Frame NavigationFrame => _navigationServiceEx.Frame;

        public MainWindow(BaseViewModel dataContext, INavigationServiceEx navigationServiceEx)
        {
            DataContext = dataContext;
            _navigationServiceEx = navigationServiceEx;

            InitializeComponent();
        }

        private void TransitioningContentControl_TransitionCompleted(object sender, RoutedEventArgs e)
        {
            ((ShellViewModel)DataContext).OnTransitionComplete();
        }

        private void HomeView_Loaded(object sender, RoutedEventArgs e)
        {

        }

        //private void HamburgerMenuControl_OnItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        //{
        //    if (e.InvokedItem is MenuItem menuItem && menuItem.IsNavigation)
        //    {
        //        this.navigationServiceEx.Navigate(menuItem.NavigationDestination);
        //    }
        //}

        //private void NavigationServiceEx_OnNavigated(object sender, NavigationEventArgs e)
        //{
        //    // select the menu item
        //    //this.HamburgerMenuControl.SelectedItem = this.HamburgerMenuControl
        //    //                                             .Items
        //    //                                             .OfType<MenuItem>()
        //    //                                             .FirstOrDefault(x => x.NavigationDestination == e.Uri);
        //    //this.HamburgerMenuControl.SelectedOptionsItem = this.HamburgerMenuControl
        //    //                                                    .OptionsItems
        //    //                                                    .OfType<MenuItem>()
        //    //                                                    .FirstOrDefault(x => x.NavigationDestination == e.Uri);

        //    // or when using the NavigationType on menu item
        //    this.HamburgerMenuControl.SelectedItem = this.HamburgerMenuControl
        //                                                 .Items
        //                                                 .OfType<MenuItem>()
        //                                                 .FirstOrDefault(x => x.NavigationType == e.Content?.GetType());
        //    this.HamburgerMenuControl.SelectedOptionsItem = this.HamburgerMenuControl
        //                                                        .OptionsItems
        //                                                        .OfType<MenuItem>()
        //                                                        .FirstOrDefault(x => x.NavigationType == e.Content?.GetType());

        //}

        //private void GoBack_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (this.navigationServiceEx.CanGoBack) this.navigationServiceEx.GoBack();
        //}
    }
}

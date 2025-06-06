using MahApps.Metro.IconPacks;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.ViewModels.Factory;
using Mhyrenz_Interface.Views;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows.Input;
using System.Windows.Navigation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Mhyrenz_Interface.ViewModels
{
    public class ShellViewModel : BaseViewModel
    {
        private readonly INavigationServiceEx _navigationServiceEx;
        private readonly IViewModelFactory _viewModelFactory;

        private static readonly ObservableCollection<MenuItem> AppMenu = new ObservableCollection<MenuItem>();
        private static readonly ObservableCollection<MenuItem> AppOptionsMenu = new ObservableCollection<MenuItem>();

        public BaseViewModel CurrentViewModel => _navigationServiceEx.CurrentViewModel;

        public ObservableCollection<MenuItem> Menu => AppMenu;

        public ObservableCollection<MenuItem> OptionsMenu => AppOptionsMenu;

        public ICommand GoBackCommand { get; }
        public ICommand NavigateCommand { get; }

        private MenuItem _selectedMenuItem;
        public MenuItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set => SetProperty(ref _selectedMenuItem, value);
        }

        private MenuItem _selectedOptionsMenuItem;
        public MenuItem SelectedOptionsMenuItem
        {
            get => _selectedOptionsMenuItem;
            set => SetProperty(ref _selectedOptionsMenuItem, value);
        }

        public ShellViewModel(INavigationServiceEx navigationServiceEx, IViewModelFactory viewModelFactory)
        {
            _navigationServiceEx = navigationServiceEx;
            _navigationServiceEx.Navigated += _OnNavigated;

            _navigationServiceEx.Navigate(new Uri("Views/HomeView.xaml", UriKind.RelativeOrAbsolute));

            NavigateCommand = new RelayCommand(_Navigate);
            GoBackCommand = new RelayCommand(execute: _GoBack, canExecute: _ => _navigationServiceEx.CanGoBack);

            _viewModelFactory = viewModelFactory;

            // Build the menus
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.HouseSolid },
                Label = "Home",
                NavigationType = typeof(HomeView),
                NavigationDestination = new Uri("Views/HomeView.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FolderSolid },
                Label = "Inventory",
                NavigationType = typeof(Inventory),
                NavigationDestination = new Uri("Views/Inventory.xaml", UriKind.RelativeOrAbsolute)
            });
            this.OptionsMenu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.GearSolid },
                Label = "Settings",
                NavigationType = typeof(Settings),
                NavigationDestination = new Uri("Views/Settings.xaml", UriKind.RelativeOrAbsolute)
            });
        }

        private void _Navigate(object parameter)
        {
            if (parameter is MenuItem menuItem && menuItem.IsNavigation)
            {
                _navigationServiceEx.Navigate(menuItem.NavigationDestination);
            }
        }

        private void _GoBack(object parameter)
        {
            _navigationServiceEx.GoBack();
        }

        private void _OnNavigated(object sender, NavigationEventArgs e)
        {
            var contentType = e.Content?.GetType();

            SelectedMenuItem = Menu.FirstOrDefault(x => x.NavigationType == contentType);
            SelectedOptionsMenuItem = OptionsMenu.FirstOrDefault(x => x.NavigationType == contentType);

            UpdateCurrentViewModel(contentType);
        }

        private void UpdateCurrentViewModel(Type viewType)
        {
            _navigationServiceEx.CurrentViewModel = _viewModelFactory.CreateViewModel(viewType);
            OnPropertyChanged(nameof(CurrentViewModel));
        }

    }
}

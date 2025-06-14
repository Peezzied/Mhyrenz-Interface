using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using Mhyrenz_Interface.Converters;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using Mhyrenz_Interface.Views;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows.Input;
using System.Windows.Navigation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using MenuItem = Mhyrenz_Interface.Controls.MenuItem;

namespace Mhyrenz_Interface.ViewModels
{
    public class ShellViewModel : BaseViewModel
    {
        private readonly INavigationServiceEx _navigationServiceEx;
        private readonly IViewModelFactory<NavigationViewModel> _viewModelFactory;

        private static readonly ObservableCollection<MenuItem> AppMenu = new ObservableCollection<MenuItem>();
        private static readonly ObservableCollection<MenuItem> AppOptionsMenu = new ObservableCollection<MenuItem>();
        private readonly IInventroyStore _inventoryStore;
        private readonly ITransactionStore _transactionStore;
        private readonly IProductService _productService;
        private readonly ITransactionsService _transactionService;

        private bool Flag { get; set; }

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

        public ShellViewModel(
            IInventroyStore inventroyStore,
            IProductService productService,
            ITransactionsService transactionService,
            ITransactionStore transactionStore,
            INavigationServiceEx navigationServiceEx,
            IViewModelFactory<NavigationViewModel> viewModelFactory)
        {
            _navigationServiceEx = navigationServiceEx;
            _navigationServiceEx.Navigated += OnNavigated;

            Flag = false;

            _navigationServiceEx.Navigate(new Uri("Views/HomeView.xaml", UriKind.RelativeOrAbsolute));

            NavigateCommand = new RelayCommand<NavigationCommandParams>(Navigate);
            GoBackCommand = new RelayCommand(execute: GoBack, canExecute: _ => _navigationServiceEx.CanGoBack);

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
                NavigationType = typeof(InventoryView),
                NavigationDestination = new Uri("Views/InventoryView.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.ClockRotateLeftSolid },
                Label = "Transactions",
                NavigationType = typeof(TransactionsView),
                NavigationDestination = new Uri("Views/TransactionsView.xaml", UriKind.RelativeOrAbsolute)
            });
            this.OptionsMenu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.GearSolid },
                Label = "Settings",
                NavigationType = typeof(SettingsView),
                NavigationDestination = new Uri("Views/SettingsView.xaml", UriKind.RelativeOrAbsolute)
            });

            _inventoryStore = inventroyStore;
            _transactionStore = transactionStore;
            _productService = productService;
            _transactionService = transactionService;

            _productService.GetAll().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    var products = task.Result;
                    _inventoryStore.LoadProducts(products);

                    Debug.WriteLine("Loaded products: " + products);
                }
                else
                {
                    // Handle error
                    Debug.WriteLine("Failed to load products: " + task.Exception?.Message);
                }
            });

            _transactionService.GetLatests().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    var products = task.Result;
                    _transactionStore.LoadTransactions(products);

                    Debug.WriteLine("Loaded transactions: " + products);
                }
                else
                {
                    // Handle error
                    Debug.WriteLine("Failed to load transactions: " + task.Exception?.Message);
                }
            });
        }

        private void Navigate(NavigationCommandParams parameters)
        {
            var selectedItem = ReferenceEquals(parameters.MenuItem, Menu)
                ? parameters.Menu.SelectedItem
                : parameters.Menu.SelectedOptionsItem;

            if (selectedItem is MenuItem menuItem && menuItem.NavigationDestination != null && menuItem.IsNavigation)
            {
                _navigationServiceEx.Navigate(menuItem.NavigationDestination);
            }
        }

        private void GoBack(object parameter)
        {
            _navigationServiceEx.GoBack();
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            var contentType = e.Content?.GetType();
            var lastOptionsMenuItem = SelectedOptionsMenuItem;

            //Debug.WriteLine($"Navigated to: {contentType?.Name}");

            SelectedMenuItem = Menu.FirstOrDefault(x => x.NavigationType == contentType);
            SelectedOptionsMenuItem = OptionsMenu.FirstOrDefault(x => x.NavigationType == contentType);

            UpdateCurrentViewModel(contentType);
        }

        private void UpdateCurrentViewModel(Type viewType)
        {
            _navigationServiceEx.CurrentViewModel = _viewModelFactory.CreateViewModel(viewType);
            OnPropertyChanged(nameof(CurrentViewModel));

            Debug.WriteLine($"Current ViewModel updated to: {CurrentViewModel.GetType().Name}");
        }

    }
}

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Controls;
using Mhyrenz_Interface.Controls.RibbonBarTools;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;

namespace Mhyrenz_Interface.ViewModels
{
    public class HomeViewModel : NavigationViewModel, ISalesRegisterHost
    {
        private readonly IInventoryStore _inventoryStore;
        private readonly ICategoryStore _categoryStore;
        private readonly OverviewChartViewModel _overviewChartViewModel;
        private readonly ISessionStore _sessionStore;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly InfoPanelViewModel _infoPanelViewModel;
        private InventoryDataGridViewModel _invetoryDataGridContext;
        public InventoryDataGridViewModel InventoryDataGridContext
        {
            get => _invetoryDataGridContext;
            set
            {
                _invetoryDataGridContext = value;
                OnPropertyChanged(nameof(InventoryDataGridContext));
            }
        }
        public ICommand RegisterCommand { get; private set; }
        public ICommand OpenStartupCommand { get; set; }
        public OverviewChartViewModel OverviewChartViewModel => _overviewChartViewModel;
        public InfoPanelViewModel InfoPanelViewModel => _infoPanelViewModel;
        public string Bindtest { get; private set; }

        private bool _isRegistering;
        public bool IsRegistering
        {
            get => _isRegistering;
            set
            {
                _isRegistering = value;
                OnPropertyChanged(nameof(IsRegistering));
            }
        }

        public IncomingPanelViewModel IncomingPanelViewModel { get; set; }

        private int currentCount = 0;
        private readonly int maxItems = 14;

        private string _searchBar = string.Empty;
        public string SearchBar
        {
            get => _searchBar;
            set
            {
                _searchBar = value;
                OnPropertyChanged(nameof(SearchBar));

                currentCount = 0;
            }
        }

        public HomeViewModel(
            IInventoryStore inventroyStore,
            ICategoryStore categoryStore,
            ISessionStore sessionStore,
            INavigationServiceEx navigationServiceEx,
            OverviewChartViewModel overviewChartViewModel,
            IDialogCoordinator dialogCoordinator,
            IncomingPanelViewModel incomingPanelViewModel,
            ShellViewModel shellViewModel,
            CreateViewModel<InventoryDataGridViewModel> inventoryDataGridViewModelFactory) : base(navigationServiceEx)
        {
            _inventoryStore = inventroyStore;
            _categoryStore = categoryStore;
            _overviewChartViewModel = overviewChartViewModel;

            //InventoryDataGridContext = inventoryDataGridViewModelFactory(this);
            IncomingPanelViewModel = incomingPanelViewModel;

            _infoPanelViewModel = new InfoPanelViewModel(_inventoryStore);

            _sessionStore = sessionStore;
            _sessionStore.StateChanged += SessionStore_StateChanged;
            Bindtest = _sessionStore.CurrentSession?.Period.ToString("M") ?? "No Session";

            base.TransitionCompleted += OnTransitionComplete;

            _dialogCoordinator = dialogCoordinator;

            DeferLoad();

            OpenStartupCommand = new AsyncRelayCommand(OpenStartupActionCommand);
            //TODO RegisterCommand = new SalesRegisterCommand();
        }

        private void SessionStore_StateChanged(Session obj)
        {

        }

        private async Task OpenStartupActionCommand(object arg)
        {
            await App.Presenter.ShowStartUpAsync();
        }

        private void OnPromptSessionRequest()
        {
            _dialogCoordinator.ShowModalMessageExternal(this, "Hello world", "Hello world");
        }

        public override void Dispose()
        {
            //InventoryDataGridContext.Dispose();

            _overviewChartViewModel.Dispose();
            _infoPanelViewModel.Dispose();
            base.TransitionCompleted -= OnTransitionComplete;
        }

        private void OnTransitionComplete()
        {
            //App.Current.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    Inventory = _inventoryStore.ProductsCollectionView;
            //    OnPropertyChanged(nameof(Inventory));

            //    Transactions = CollectionViewSource.GetDefaultView(_transactionStore.Transactions);
            //    OnPropertyChanged(nameof(Transactions));
            //}), DispatcherPriority.ContextIdle);
        }

        private void DeferLoad()
        {
            //InventoryDataGridContext.Inventory = CollectionViewSource.GetDefaultView(_inventoryStore.Products);
            //InventoryDataGridContext.Inventory.Filter += FilterProducts;
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {

                if (_categoryStore.Colors.Any())
                    foreach (var item in _inventoryStore.Store)
                    {
                        item.CategoryColor = _categoryStore.Colors[item.CategoryId];
                    }

            }), DispatcherPriority.ContextIdle);
        }

        private bool FilterProducts(object obj)
        {
            if (obj is ProductDataViewModel productDataViewModel)
            {
                var search = productDataViewModel.Name?.IndexOf(SearchBar, StringComparison.InvariantCultureIgnoreCase) >= 0;

                if (!search)
                    return false;

                if (currentCount >= maxItems)
                    return false;

                currentCount++;
                return true;

            }
            else return false;

        }

    }

    public interface ISalesRegisterHost
    {
        bool IsRegistering { get; set; }
    }
}

using HandyControl.Tools.Extension;
using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Domain.State.Mediator;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Mhyrenz_Interface.ViewModels
{
    public class HomeViewModel: NavigationViewModel
    {
        private readonly IInventoryStore _inventoryStore;
        private readonly ICategoryStore _categoryStore;
        private readonly ITransactionStore _transactionStore;
        private readonly INavigationServiceEx _navigationServiceEx;
        private readonly OverviewChartViewModel _overviewChartViewModel;
        private readonly ISalesRecordService _salesRecordService;
        private readonly ITransactionsService _transactionService;
        private readonly IViewModelFactory<InventoryDataGridViewModel> _inventoryDataGridViewModelFactory;
        private readonly ISessionStore _sessionStore;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly InfoPanelViewModel _infoPanelViewModel;
        private InventoryDataGridViewModel _invetoryDataGridContext;
        public InventoryDataGridViewModel InventoryDataGridContext
        {
            get =>  _invetoryDataGridContext;
            set
            {
                _invetoryDataGridContext = value;
                OnPropertyChanged(nameof(InventoryDataGridContext));
            }
        }
        private ICollectionView Inventory;
        public ICommand RegisterCommand { get; private set; }
        public ICollectionView Transactions { get; private set; }
        public OverviewChartViewModel OverviewChartViewModel => _overviewChartViewModel;
        public InfoPanelViewModel InfoPanelViewModel => _infoPanelViewModel;
        public string Bindtest => _sessionStore.CurrentSession?.Period.ToString("M") ?? "No Session";

        private bool _isLoading = false;
        public bool IsLoading 
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

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

                _inventoryStore.ProductsCollectionView.Refresh();
                currentCount = 0;
            }
        }

        public HomeViewModel(
            ISalesRecordService salesRecordService,
            ITransactionsService transactionsService,
            ITransactionStore transactionStore,
            IInventoryStore inventroyStore,
            ICategoryStore categoryStore,
            ISessionStore sessionStore,
            INavigationServiceEx navigationServiceEx,
            OverviewChartViewModel overviewChartViewModel,
            IDialogCoordinator dialogCoordinator,
            IViewModelFactory<InventoryDataGridViewModel> inventoryDataGridViewModelFactory) : base(navigationServiceEx)
        {
            _navigationServiceEx = navigationServiceEx;
            _inventoryStore = inventroyStore;
            _categoryStore = categoryStore;
            _transactionStore = transactionStore;
            _overviewChartViewModel = overviewChartViewModel;
            _salesRecordService = salesRecordService;
            _transactionService = transactionsService;

            _inventoryDataGridViewModelFactory = inventoryDataGridViewModelFactory;
            InventoryDataGridContext = _inventoryDataGridViewModelFactory.CreateViewModel();

            _infoPanelViewModel = new InfoPanelViewModel(_inventoryStore);

            _sessionStore = sessionStore;
            _inventoryStore.ProductsCollectionView.Filter += FilterProducts;

            base.TransitionCompleted += OnTransitionComplete;
            _inventoryStore.PromptSessionEvent += OnPromptSessionRequest;
            _sessionStore.StateChanged += OnCurrentSession;

            _dialogCoordinator = dialogCoordinator;

            DeferLoad();

            RegisterCommand = new SalesRegisterCommand(this, _salesRecordService, _transactionStore, _transactionService, _sessionStore, inventroyStore);
        }

        private void OnCurrentSession()
        {
            OnPropertyChanged(nameof(Bindtest));
        }

        private void OnPromptSessionRequest()
        {
            _dialogCoordinator.ShowModalMessageExternal(this, "Hello world", "Hello world");
        }

        public override void Dispose()
        {
            base.TransitionCompleted -= OnTransitionComplete;
            _inventoryStore.PromptSessionEvent -= OnPromptSessionRequest;
            _sessionStore.StateChanged -= OnCurrentSession;
            _inventoryStore.ProductsCollectionView.Filter -= FilterProducts;

            _overviewChartViewModel.Dispose();
            _infoPanelViewModel.Dispose();
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

        private async void DeferLoad()
        {
            await App.Current.Dispatcher.InvokeAsync(new Action(() =>
            {
                Inventory = _inventoryStore.ProductsCollectionView;
                InventoryDataGridContext.Inventory = Inventory;

                foreach (var item in _inventoryStore.Products)
                {
                    item.CategoryColor = _categoryStore.Colors[item.CategoryId];
                }

                Transactions = CollectionViewSource.GetDefaultView(_transactionStore.Transactions);
                OnPropertyChanged(nameof(Transactions));
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
}

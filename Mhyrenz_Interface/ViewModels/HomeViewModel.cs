﻿using HandyControl.Tools.Extension;
using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
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
    public class HomeViewModel: NavigationViewModel, SalesRegisterHost
    {
        private readonly IInventoryStore _inventoryStore;
        private readonly ICategoryStore _categoryStore;
        private readonly ITransactionStore _transactionStore;
        private readonly INavigationServiceEx _navigationServiceEx;
        private readonly OverviewChartViewModel _overviewChartViewModel;
        private readonly ISalesRecordService _salesRecordService;
        private readonly ITransactionsService _transactionService;
        private readonly CreateViewModel<InventoryDataGridViewModel> _inventoryDataGridViewModelFactory;
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
        public ICommand RegisterCommand { get; private set; }
        public ICommand OpenStartupCommand { get; set; }
        private ICollectionView _transactions;
        public ICollectionView Transactions
        {
            get => _transactions;
            set
            {
                _transactions = value;
                OnPropertyChanged(nameof(Transactions));
            }
        }
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
            CreateViewModel<InventoryDataGridViewModel> inventoryDataGridViewModelFactory) : base(navigationServiceEx)
        {
            _navigationServiceEx = navigationServiceEx;
            _inventoryStore = inventroyStore;
            _categoryStore = categoryStore;
            _transactionStore = transactionStore;
            _overviewChartViewModel = overviewChartViewModel;
            _salesRecordService = salesRecordService;
            _transactionService = transactionsService;

            _inventoryDataGridViewModelFactory = inventoryDataGridViewModelFactory;
            InventoryDataGridContext = _inventoryDataGridViewModelFactory(this);

            _infoPanelViewModel = new InfoPanelViewModel(_inventoryStore);

            _sessionStore = sessionStore;
            _sessionStore.StateChanged += _sessionStore_StateChanged;
            Bindtest = _sessionStore.CurrentSession?.Period.ToString("M") ?? "No Session";
            //_inventoryStore.ProductsCollectionView.Filter += FilterProducts;

            base.TransitionCompleted += OnTransitionComplete;
            _inventoryStore.PromptSessionEvent += OnPromptSessionRequest;

            _dialogCoordinator = dialogCoordinator;

            DeferLoad();

            OpenStartupCommand = new AsyncRelayCommand(OpenStartupActionCommand);
            RegisterCommand = new SalesRegisterCommand(this, _salesRecordService, _transactionStore, _transactionService, _sessionStore, inventroyStore);
        }

        private void _sessionStore_StateChanged(Session obj)
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
            base.TransitionCompleted -= OnTransitionComplete;
            _inventoryStore.PromptSessionEvent -= OnPromptSessionRequest;
            _inventoryStore.ProductsCollectionView.Filter -= FilterProducts;

            InventoryDataGridContext.Dispose();
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

        private void DeferLoad()
        {
                InventoryDataGridContext.Inventory = CollectionViewSource.GetDefaultView(_inventoryStore.Products);
            InventoryDataGridContext.Inventory.Filter += FilterProducts;
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {

                if (_categoryStore.Colors.Any())
                    foreach (var item in _inventoryStore.Products)
                    {
                        item.CategoryColor = _categoryStore.Colors[item.CategoryId];
                    }

            }), DispatcherPriority.ContextIdle);
                Transactions = CollectionViewSource.GetDefaultView(_transactionStore.Transactions);
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

    public interface SalesRegisterHost
    {
        bool IsRegistering { get; set; }
    }
}

using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
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
using System.Windows.Threading;

namespace Mhyrenz_Interface.ViewModels
{
    public class HomeViewModel: NavigationViewModel
    {
        private readonly IInventoryStore _inventoryStore;
        private readonly ITransactionStore _transactionStore;
        private readonly INavigationServiceEx _navigationServiceEx;
        private readonly OverviewChartViewModel _overviewChartViewModel;
        private readonly ISessionStore _sessionStore;
        private readonly IDialogCoordinator _dialogCoordinator;

        public ICollectionView Inventory { get; private set; }
        public ICollectionView Transactions { get; private set; }
        public OverviewChartViewModel OverviewChartViewModel => _overviewChartViewModel;   
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

        private string _searchBar = string.Empty;
        public string SearchBar 
        {
            get => _searchBar;
            set
            {
                _searchBar = value;
                OnPropertyChanged(nameof(SearchBar));
                _inventoryStore.ProductsCollectionView.Refresh();
            }
        }

        //public string SearchBarTest { get; set; } = "asda";

        public HomeViewModel(
            ITransactionStore transactionStore,
            IInventoryStore inventroyStore,
            ISessionStore sessionStore,
            INavigationServiceEx navigationServiceEx,
            OverviewChartViewModel overviewChartViewModel,
            IDialogCoordinator dialogCoordinator) : base(navigationServiceEx)
        {


            _navigationServiceEx = navigationServiceEx;
            _inventoryStore = inventroyStore;
            _transactionStore = transactionStore;
            _overviewChartViewModel = overviewChartViewModel;

            _sessionStore = sessionStore;
            _inventoryStore.ProductsCollectionView.Filter = FilterProducts;

            base.TransitionCompleted += OnTransitionComplete;
            _inventoryStore.PromptSessionEvent += OnPromptSessionRequest;

            _dialogCoordinator = dialogCoordinator;
        }

        private void OnPromptSessionRequest()
        {
            _dialogCoordinator.ShowModalMessageExternal(this, "Hello world", "Hello world");
        }

        public override void Dispose()
        {
            base.TransitionCompleted -= OnTransitionComplete;
            _inventoryStore.PromptSessionEvent -= OnPromptSessionRequest;
        }

        private void OnTransitionComplete()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Inventory = _inventoryStore.ProductsCollectionView;
                OnPropertyChanged(nameof(Inventory));

                Transactions = CollectionViewSource.GetDefaultView(_transactionStore.Transactions);
                OnPropertyChanged(nameof(Transactions));
            }), DispatcherPriority.ContextIdle);
        }

        private bool FilterProducts(object obj)
        {
            if (obj is ProductDataViewModel productDataViewModel)
            {
                return productDataViewModel.Name?.IndexOf(SearchBar, StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
            else return false;
            
        }



        //private async void LoadProducts()
        //{
        //    var products = await _productService.GetAll();
        //    Inventory?.Clear();
        //    foreach (var product in products)
        //    {
        //        Inventory.Add(product);
        //    }
        //}

    }
}

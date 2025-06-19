using Mhyrenz_Interface;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
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

        public ICollectionView Inventory { get; private set; }
        public ICollectionView Transactions { get; private set; }
        public OverviewChartViewModel OverviewChartViewModel => _overviewChartViewModel;   
        public string Bindtest { get; set; } = "Hello, World!";

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
            INavigationServiceEx navigationServiceEx,
            OverviewChartViewModel overviewChartViewModel) : base(navigationServiceEx)
        {
            _navigationServiceEx = navigationServiceEx;
            _inventoryStore = inventroyStore;
            _transactionStore = transactionStore;
            _overviewChartViewModel = overviewChartViewModel;

            _inventoryStore.ProductsCollectionView.Filter = FilterProducts;

            base.TransitionCompleted += OnTransitionComplete;

        }

        public override void Dispose()
        {
            base.TransitionCompleted -= OnTransitionComplete;
        }

        private void OnTransitionComplete()
        {
            Inventory = _inventoryStore.ProductsCollectionView;
            OnPropertyChanged(nameof(Inventory));

            Transactions = CollectionViewSource.GetDefaultView(_transactionStore.Transactions);
            OnPropertyChanged(nameof(Transactions));
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

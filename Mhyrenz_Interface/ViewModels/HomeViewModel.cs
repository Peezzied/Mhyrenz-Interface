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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.ViewModels
{
    public class HomeViewModel: NavigationViewModel
    {
        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;
        private readonly ITransactionStore _transactionStore;
        private readonly INavigationServiceEx _navigationServiceEx;
        private readonly OverviewChartViewModel _overviewChartViewModel;

        public ObservableCollection<ProductDataViewModel> Inventory => _inventoryStore.Products;
        public ObservableCollection<TransactionDataViewModel> Transactions => _transactionStore.Transactions;
        public OverviewChartViewModel OverviewChartViewModel => _overviewChartViewModel;   
        public string Bindtest { get; set; } = "Hello, World!";

        public HomeViewModel(
            IProductService productService,
            ITransactionStore transactionStore,
            IInventoryStore inventroyStore,
            INavigationServiceEx navigationServiceEx,
            OverviewChartViewModel overviewChartViewModel) : base(navigationServiceEx)
        {
            _navigationServiceEx = navigationServiceEx;

            _productService = productService;
            _inventoryStore = inventroyStore;
            _transactionStore = transactionStore;
            _overviewChartViewModel = overviewChartViewModel;
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

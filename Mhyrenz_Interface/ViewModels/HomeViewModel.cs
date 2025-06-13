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
        private readonly IInventroyStore _inventoryStore;
        private readonly INavigationServiceEx _navigationServiceEx;

        public ObservableCollection<ProductViewModel> Inventory => _inventoryStore.Products;

        public string Bindtest { get; set; } = "Hello, World!";

        public HomeViewModel(IProductService productService, IInventroyStore inventroyStore, INavigationServiceEx navigationServiceEx) : base(navigationServiceEx)
        {
            _navigationServiceEx = navigationServiceEx;

            _productService = productService;
            _inventoryStore = inventroyStore;

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

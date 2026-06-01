using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;

namespace Mhyrenz_Interface.State
{
    public class InventoryStore : IInventoryStore
    {
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly InventorySettingsProvider _inventorySettingsProvider;
        private readonly CreateCommand<DirectPurchaseCommand> _directPurchaseCommand;
        private readonly CreateViewModel<ProductDataViewModel> _productsViewModelFactory;
        private readonly IProductService _productService;
        private readonly ICheckoutService _checkoutService;
        private readonly ISessionStore _sessionStore;
        private readonly INavigationServiceEx _navigationService;
        private readonly NavigationViewModelFactory _navigationViewModelFactory;

        public SourceCollection<int, ProductDataViewModel> Store { get; } = new SourceCollection<int, ProductDataViewModel>(
            v => v.Item.Id);

        public event EventHandler<InventoryStoreEventArgs> PropertyChanged;
        public event EventHandler<InventoryStoreEventArgs> PurchaseEvent;
        public event EventHandler<IEnumerable<ProductDataViewModel>> AddProductEvent;
        public event EventHandler<IEnumerable<ProductDataViewModel>> RemoveProductEvent;
        public event Action Loaded;

        public InventoryStore(
            IUndoRedoManager undoRedoManager,
            InventorySettingsProvider inventorySettingsProvider,
            CreateCommand<DirectPurchaseCommand> directPurchaseCommand,
            CreateViewModel<ProductDataViewModel> productsViewModelFactory,
            IProductService productService,
            ICheckoutService checkoutService,
            ISessionStore sessionStore,
            INavigationServiceEx navigationServiceEx,
            NavigationViewModelFactory navigationViewModelFactory)
        {
            _undoRedoManager = undoRedoManager;
            _inventorySettingsProvider = inventorySettingsProvider;
            _directPurchaseCommand = directPurchaseCommand;
            _productsViewModelFactory = productsViewModelFactory;
            _productService = productService;
            _checkoutService = checkoutService;
            _sessionStore = sessionStore;
            _navigationService = navigationServiceEx;
            _navigationViewModelFactory = navigationViewModelFactory;
        }

        #region "Lifecycle and Instantiation"
        public async Task InitializeAsync()
        {
            var products = await _productService.GetAll();

            var extrasTemplates = _inventorySettingsProvider.ColumnSchemaMap
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToDictionary(c => c.Field, c => (object)null)
                );

            LoadProducts(products);
        }

        public static async Task LoadInventoryStore(IServiceProvider serviceProvider)
        {
            var inventoryStore = serviceProvider.GetRequiredService<IInventoryStore>();
            await inventoryStore.InitializeAsync();
        }
        public void LoadProducts(IEnumerable<Product> products)
        {
            var displayProducts = products
                .Select(product => _productsViewModelFactory(product))
                .ToList();

            Store.Clear();

            Store.AddRange(displayProducts);

            Loaded?.Invoke();
        }
        #endregion

        [Obsolete("Not still implemented yet")]
        public async Task Register(IEnumerable<Product> transactions)
        {
            // TODO implement registering transaction with trasaction service

            var products = await _productService.GetAll();

            LoadProducts(products);
        }

        public void RemoveProduct(IEnumerable<ProductDataViewModel> products)
        {
            RemoveProductEvent?.Invoke(this, products);
            Store.RemoveMany(products.Select(x => x.Item.Id));

        }

        public IEnumerable<ProductDataViewModel> AddProduct(ICollection<Product> products)
        {
            var displayProducts = products.Select(product => _productsViewModelFactory(product))
                .ToList();

            Store.AddRange(displayProducts);

            AddProductEvent?.Invoke(this, displayProducts);

            return displayProducts;
        }

    }

    public class InventoryStoreEventArgs
    {
        public int ProductId { get; set; }
        public ProductDataViewModel Product { get; set; }
        public string PropertyName { get; internal set; }
    }
}


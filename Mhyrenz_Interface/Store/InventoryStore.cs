using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.Collection;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.Store
{
    public class InventoryStore : IInventoryStore
    {
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly CreateViewModel<ProductDataViewModel> _productsViewModelFactory;
        private readonly IProductService _productService;

        public SourceCollection<int, ProductDataViewModel> Store { get; } = new SourceCollection<int, ProductDataViewModel>(
            v => v.Item.Id);

        public event EventHandler<InventoryStoreEventArgs> PropertyChanged;
        public event EventHandler<InventoryStoreEventArgs> PurchaseEvent;
        public event EventHandler<IEnumerable<ProductDataViewModel>> AddProductEvent;
        public event EventHandler<IEnumerable<ProductDataViewModel>> RemoveProductEvent;
        public event Action Loaded;

        public InventoryStore(
            IUndoRedoManager undoRedoManager,
            CreateViewModel<ProductDataViewModel> productsViewModelFactory,
            IProductService productService)
        {
            _undoRedoManager = undoRedoManager;
            _productsViewModelFactory = productsViewModelFactory;
            _productService = productService;
        }

        #region "Lifecycle and Instantiation"
        public async Task InitializeAsync()
        {
            var products = await _productService.GetAll();

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

        public void PurchaseProduct(int productId, int amount)
        {
            if (Store.TryGetValue(productId, out var product))
            {
                product.Purchase = amount;

                PurchaseEvent?.Invoke(this, new InventoryStoreEventArgs
                {
                    ProductId = productId,
                    Product = product
                });
            }
        }

    }

    public class InventoryStoreEventArgs
    {
        public int ProductId { get; set; }
        public ProductDataViewModel Product { get; set; }
    }
}


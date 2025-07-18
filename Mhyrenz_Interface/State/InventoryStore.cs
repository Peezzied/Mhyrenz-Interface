using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Diagnostics;
using System.Windows.Input;

namespace Mhyrenz_Interface.State
{
    public class InventoryStore : IInventoryStore
    {
        private readonly UndoRedoManager _undoRedoManager;
        private readonly CreateViewModel<ProductDataViewModel> _productsViewModelFactory;
        private readonly IProductService _productService;
        private readonly ITransactionsService _transactionService;
        private readonly ISessionStore _sessionStore;
        private readonly List<PropertyChangeTracker<ProductDataViewModel>> _trackers = new List<PropertyChangeTracker<ProductDataViewModel>>();

        public ObservableCollection<ProductDataViewModel> Products { get; } = new ObservableCollection<ProductDataViewModel>();
        public ICollectionView ProductsCollectionView { get; set; }
        public ILookup<string, ProductDataViewModel> ProductsCollectionViewByCategory { get; set; }
        public ICommand UpdateProductCommand { get; set; }
        public ICommand PurchaseProductCommand { get; set; }
        public InventoryStore(
            UndoRedoManager undoRedoManager,
            CreateViewModel<ProductDataViewModel> productsViewModelFactory,
            IProductService productService,
            ITransactionsService transactionsService,
            ISessionStore sessionStore)
        {
            _undoRedoManager = undoRedoManager;
            _productsViewModelFactory = productsViewModelFactory;
            _productService = productService;
            _transactionService = transactionsService;
            _sessionStore = sessionStore;
            //_categoryStore = categoryStore;

            UpdateProductCommand = new UpdateProductCommand(_productService, this);
            PurchaseProductCommand = new PurchaseProductCommand(_transactionService, this);
        }

        public ProductDataViewModel AddProduct(Product product)
        {
            var productVm = _productsViewModelFactory(new ProductDataViewModelDTO
            {
                Product = product,
                SessionStore = _sessionStore,
                OnSessionNull = PromptSession
            });

            _trackers.Add(TrackProducts(productVm));
            Products.Add(productVm);

            AddProductEvent?.Invoke(this, productVm);

            return productVm;
        }

        public void LoadProducts(IEnumerable<Product> products)
        {

            App.Current.Dispatcher.Invoke(() =>
            {
                Products.Clear();
                _trackers.Clear();
                ChangeTracking.IsInventoryLoaded = true;

                var displayProducts = products.Select(product => _productsViewModelFactory(new ProductDataViewModelDTO
                {
                    Product = product,
                    SessionStore = _sessionStore,
                    OnSessionNull = PromptSession
                }));

                foreach (var item in displayProducts)
                {
                    _trackers.Add(TrackProducts(item));
                    Products.Add(item);
                }

                ProductsCollectionView = CollectionViewSource.GetDefaultView(Products);

                Loaded?.Invoke();

            });
        }

        #region "Helper"
        private void PromptSession()
        {
            PromptSessionEvent?.Invoke();
        }

        private PropertyChangeTracker<ProductDataViewModel> TrackProducts(ProductDataViewModel viewModel)
        {
            var _tracker = new PropertyChangeTracker<ProductDataViewModel>(viewModel);
            Action<PropertyChangeTracker<ProductDataViewModel>, TargetChangedEventArgs, object, object> method;


            // Track changes to properties and execute commands on change
            // * Commons
            method = (tracker, args, oldValue, newValue) =>
            {
                _undoRedoManager.Execute(new ProductVMCommandCommon(
                    viewModel,
                    args.PropertyOf,
                    oldValue,
                    newValue,
                    UpdateProductCommand
                ));

                HandlePropertyChanged(tracker, args, (vm, product, _) =>
                {
                    PropertyChanged?.Invoke(vm, new InventoryStoreEventArgs()
                    {
                        ProductId = product.Id
                    });
                });
            };
            _tracker
                .Track(nameof(ProductDataViewModel.Qty), viewModel.Qty, method)
                .Track(nameof(ProductDataViewModel.Name), viewModel.Name, method)
                .Track(nameof(ProductDataViewModel.Barcode), viewModel.Barcode, method)
                .Track(nameof(ProductDataViewModel.Expiry), viewModel.Expiry, method)
                .Track(nameof(ProductDataViewModel.Batch), viewModel.Batch, method);

            // * EditPurchase

            Action<object, TargetChangedEventArgs> purchaseHandlePropChange = new Action<object, TargetChangedEventArgs>((tracker, args) =>
            {
                HandlePropertyChanged(tracker, args, (vm, product, index) =>
                {
                    var updated = _productsViewModelFactory(new ProductDataViewModelDTO
                    {
                        Product = product,
                        SessionStore = _sessionStore,
                        OnSessionNull = PromptSession
                    });
                    PurchaseEvent?.Invoke(vm, new InventoryStoreEventArgs()
                    {
                        ProductId = product.Id,
                        Product = updated
                    });

                    Products.RemoveAt(index);
                    Products.Insert(index, updated);

                });
            });
            method = (tracker, args, oldValue, newValue) =>
            {
                void handlePropChange() => purchaseHandlePropChange(tracker, args);

                _undoRedoManager.Execute(new ProductVMCommandPurchase(
                    viewModel,
                    args.PropertyOf,
                    oldValue,
                    newValue,
                    command: PurchaseProductCommand,
                    propertyChangeHandler: handlePropChange
                ));
            };
            _tracker
                .Track(nameof(ProductDataViewModel.PurchaseDefaultEdit), viewModel.PurchaseDefaultEdit, method)
                .Track(nameof(ProductDataViewModel.PurchaseNormalEdit), viewModel.PurchaseNormalEdit, method);

            return _tracker;
        }

        private async void HandlePropertyChanged(
            object sender,
            TargetChangedEventArgs e,
            Action<ProductDataViewModel, Product, int> propChanged = null)
        {
            var tracker = (PropertyChangeTracker<ProductDataViewModel>)sender;
            var target = (ProductDataViewModel)e.Target;
            var propertyOf = e.PropertyOf;

            if (!!!tracker.PreviousValues.TryGetValue(propertyOf, out var previousValue) && previousValue is null)
                return;

            int index = Products.IndexOf(Products.FirstOrDefault(x => ReferenceEquals(x, target)));

            if (index >= 0)
            {
                var product = await _productService.Get(target.Item.Id);

                await App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _trackers.Remove(tracker);

                    propChanged?.Invoke(Products[index], product, index);

                    _trackers.Add(TrackProducts(Products[index]));
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }
        #endregion

        public async Task Register(IEnumerable<Product> transactions)
        {
            // SLOW TIME COMPLEXITY - RESOLVE LATER

            var tasks = transactions.Select(item =>
                _productService.Edit(item.Id, nameof(Product.Qty), item.Qty) // resolve a batch edit
            );

            await Task.WhenAll(tasks);

            var products = await _productService.GetAll();

            LoadProducts(products);
        }

        public event EventHandler<InventoryStoreEventArgs> PropertyChanged;
        public event EventHandler<InventoryStoreEventArgs> PurchaseEvent;
        public event Action PromptSessionEvent;
        public event EventHandler<ProductDataViewModel> AddProductEvent;
        public event Action Loaded;

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
    }

    public class InventoryStoreEventArgs
    {
        public int ProductId { get; set; }
        public ProductDataViewModel Product { get; set; }
    }
}


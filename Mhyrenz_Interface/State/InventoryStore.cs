using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Mhyrenz_Interface.Views;
using Microsoft.EntityFrameworkCore.Internal;
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
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly CreateViewModel<ProductDataViewModel> _productsViewModelFactory;
        private readonly IProductService _productService;
        private readonly ITransactionsService _transactionService;
        private readonly ISessionStore _sessionStore;
        private readonly INavigationServiceEx _navigationService;
        private readonly NavigationViewModelFactory _navigationViewModelFactory;
        private readonly Dictionary<ProductDataViewModel, PropertyChangeTracker<ProductDataViewModel>> _trackers =
            new Dictionary<ProductDataViewModel, PropertyChangeTracker<ProductDataViewModel>>();

        public ObservableCollection<ProductDataViewModel> Products { get; } = new SmartObservableCollection<ProductDataViewModel>();
        public ICollectionView ProductsCollectionView { get; set; }
        public ILookup<string, ProductDataViewModel> ProductsCollectionViewByCategory { get; set; }
        public ICommand UpdateProductCommand { get; set; }
        public ICommand PurchaseProductCommand { get; set; }
        public (int Index, ProductDataViewModel Product) LastProductChanged { get; set; }

        public InventoryStore(
            IUndoRedoManager undoRedoManager,
            CreateViewModel<ProductDataViewModel> productsViewModelFactory,
            IProductService productService,
            ITransactionsService transactionsService,
            ISessionStore sessionStore,
            INavigationServiceEx navigationServiceEx,
            NavigationViewModelFactory navigationViewModelFactory)
        {
            _undoRedoManager = undoRedoManager;
            _productsViewModelFactory = productsViewModelFactory;
            _productService = productService;
            _transactionService = transactionsService;
            _sessionStore = sessionStore;
            _navigationService = navigationServiceEx;
            _navigationViewModelFactory = navigationViewModelFactory;
            //_categoryStore = categoryStore;

            UpdateProductCommand = new UpdateProductCommand(_productService, this);
            PurchaseProductCommand = new PurchaseProductCommand(_transactionService, this);
        }
        public void RemoveProduct(ProductDataViewModel product)
        {
            LastProductChanged = (Products.IndexOf(product), product);

            _trackers.Remove(product);
            Products.Remove(product);
        }
        public void RemoveProduct(IEnumerable<ProductDataViewModel> products)
        {
            var productsMap = products.ToHashSet();
            var product = products.First();

            WithFilterSuspended(() => LastProductChanged = (ProductsCollectionView.Cast<ProductDataViewModel>().IndexOf(product), product));

            foreach (var item in products.ToList())
            {
                _trackers.Remove(_trackers.FirstOrDefault(t => t.Key.Item.Id == item.Item.Id).Key);
                Products.Remove(item);
            }
        }

        public ProductDataViewModel AddProduct(Product product)
        {
            var productVm = _productsViewModelFactory(product);

            _trackers[productVm] = TrackProduct(productVm);
            Products.Add(productVm);

            AddProductEvent?.Invoke(this, productVm);

            LastProductChanged = (Products.IndexOf(productVm), productVm);

            return productVm;
        }

        public void AddProduct(IEnumerable<Product> products)
        {
            var displayProducts = products.Select(product => _productsViewModelFactory(product));

            ProductDataViewModel productVm = default;
            var isFirst = true;
            foreach (var item in displayProducts)
            {
                _trackers[item] = TrackProduct(item);

                if (isFirst)
                {
                    productVm = item;
                    Products.Add(productVm);
                    isFirst = false;
                }
                else
                    Products.Add(item);

            }


            AddProductEvent?.Invoke(this, productVm);

            WithFilterSuspended(() => LastProductChanged = (ProductsCollectionView.Cast<ProductDataViewModel>().IndexOf(productVm), productVm));
        }

        public void LoadProducts(IEnumerable<Product> products)
        {

            App.Current.Dispatcher.Invoke(() =>
            {
                Products.Clear();
                _trackers.Clear();
                ChangeTracking.IsInventoryLoaded = true;

                var displayProducts = products.Select(product => _productsViewModelFactory(product));

                foreach (var item in displayProducts)
                {
                    _trackers[item] = TrackProduct(item);
                    Products.Add(item);
                }

                ProductsCollectionView = CollectionViewSource.GetDefaultView(Products);

                Loaded?.Invoke();

            });
        }

        public ProductDataViewModel GetProductByIndex(int index)
        {
            return ProductsCollectionView.Cast<ProductDataViewModel>().ElementAt(index);
        }

        #region "Helper"

        private void WithFilterSuspended(Action action)
        {
            var prevFilter = ProductsCollectionView.Filter;
            ProductsCollectionView.Filter = null;
            action();
            ProductsCollectionView.Filter = prevFilter;
        }

        private PropertyChangeTracker<ProductDataViewModel> TrackProduct(ProductDataViewModel viewModel)
        {
            var _tracker = new PropertyChangeTracker<ProductDataViewModel>(viewModel);
            Action<PropertyChangeTracker<ProductDataViewModel>, TargetChangedEventArgs, object, object> method;
            Action<object, TargetChangedEventArgs> propChangedHandler;


            // Track changes to properties and execute commands on change
            // * Commons
            propChangedHandler = new Action<object, TargetChangedEventArgs>((tracker, args) =>
            {
                HandlePropertyChanged(tracker, args, (vm, product, index) =>
                {
                    PropertyChanged?.Invoke(vm, new InventoryStoreEventArgs()
                    {
                        ProductId = product.Id
                    });
                    LastProductChanged = (index, Products[index]);
                });
            });
            method = (tracker, args, oldValue, newValue) =>
            {
                void handlePropChange() => propChangedHandler(tracker, args);

                _undoRedoManager.Execute(new ProductVMCommandCommon(
                    viewModel,
                    args.PropertyOf,
                    oldValue,
                    newValue,
                    UpdateProductCommand,
                    handlePropChange,
                    currentViewIn: _navigationViewModelFactory.GetViewByViewModel(_navigationService.CurrentViewModel)
                ));
            };
            _tracker
                .Track(nameof(ProductDataViewModel.Qty), viewModel.Qty, method)
                .Track(nameof(ProductDataViewModel.Name), viewModel.Name, method)
                .Track(nameof(ProductDataViewModel.Barcode), viewModel.Barcode, method)
                .Track(nameof(ProductDataViewModel.Expiry), viewModel.Expiry, method)
                .Track(nameof(ProductDataViewModel.Batch), viewModel.Batch, method);

            // * EditPurchase

            propChangedHandler = new Action<object, TargetChangedEventArgs>((tracker, args) =>
            {
                HandlePropertyChanged(tracker, args, async (vm, product, index) =>
                {
                    await App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _trackers.Remove(_trackers.FirstOrDefault(t => t.Key.Item.Id == product.Id).Key);

                        var updated = _productsViewModelFactory(product);
                        PurchaseEvent?.Invoke(vm, new InventoryStoreEventArgs()
                        {
                            ProductId = product.Id,
                            Product = updated
                        });

                        Products.RemoveAt(index);
                        Products.Insert(index, updated);


                        _trackers[Products[index]] = TrackProduct(Products[index]);
                    }), System.Windows.Threading.DispatcherPriority.Input);

                    LastProductChanged = (index, Products[index]);
                });
            });
            method = (tracker, args, oldValue, newValue) =>
            {
                void handlePropChange() => propChangedHandler(tracker, args);

                _undoRedoManager.Execute(new ProductVMCommandPurchase(
                    viewModel,
                    args.PropertyOf,
                    oldValue,
                    newValue,
                    command: PurchaseProductCommand,
                    propertyChangeHandler: handlePropChange,
                    currentViewIn: typeof(InventoryView)
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

            int index = Products.IndexOf(Products.FirstOrDefault(x => x.Item.Id == target.Item.Id));

            if (index >= 0)
            {
                var product = await _productService.Get(target.Item.Id);

                propChanged?.Invoke(Products[index], product, index);
            }
        }
        #endregion

        public async Task Register(IEnumerable<Product> transactions)
        {
            // SLOW TIME COMPLEXITY - RESOLVE LATER

            var tasks = transactions.Select(item =>
                _productService.EditProperty(item.Id, nameof(Product.Qty), item.Qty) // resolve a batch edit
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


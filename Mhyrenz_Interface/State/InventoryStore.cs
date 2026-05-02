using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
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
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.State
{
    public class InventoryStore : IInventoryStore
    {
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly InventorySettingsProvider _inventorySettingsProvider;
        private readonly CreateViewModel<ProductDataViewModel> _productsViewModelFactory;
        private readonly IProductService _productService;
        private readonly ITransactionsService _transactionService;
        private readonly ISessionStore _sessionStore;
        private readonly INavigationServiceEx _navigationService;
        private readonly NavigationViewModelFactory _navigationViewModelFactory;
        private readonly Dictionary<ProductDataViewModel, PropertyChangeTracker<ProductDataViewModel>> _trackers =
            new Dictionary<ProductDataViewModel, PropertyChangeTracker<ProductDataViewModel>>();

        public ObservableCollection<ProductDataViewModel> Products { get; } = new SmartObservableCollection<ProductDataViewModel>();
        public ICollectionView ProductsCollectionView { get; private set; }
        public ILookup<string, ProductDataViewModel> ProductsCollectionViewByCategory { get; private set; }
        public ICommand UpdateProductCommand { get; private set; }
        public (int Category, ChangedProductInfo ChangedProductInfo) LastProductChanged { get; private set; }

        

        public event EventHandler<InventoryStoreEventArgs> PropertyChanged;
        public event EventHandler<InventoryStoreEventArgs> PurchaseEvent;
        public event EventHandler<IEnumerable<ProductDataViewModel>> AddProductEvent;
        public event EventHandler<IEnumerable<ProductDataViewModel>> RemoveProductEvent;
        public event Action Loaded;

        public InventoryStore(
            IUndoRedoManager undoRedoManager,
            InventorySettingsProvider inventorySettingsProvider,
            CreateViewModel<ProductDataViewModel> productsViewModelFactory,
            IProductService productService,
            ITransactionsService transactionsService,
            ISessionStore sessionStore,
            INavigationServiceEx navigationServiceEx,
            NavigationViewModelFactory navigationViewModelFactory)
        {
            _undoRedoManager = undoRedoManager;
            _inventorySettingsProvider = inventorySettingsProvider;
            _productsViewModelFactory = productsViewModelFactory;
            _productService = productService;
            _transactionService = transactionsService;
            _sessionStore = sessionStore;
            _navigationService = navigationServiceEx;
            _navigationViewModelFactory = navigationViewModelFactory;
            //_categoryStore = categoryStore;

            UpdateProductCommand = new UpdateProductCommand(_productService, this);
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

            foreach (var product in products)
            {
                if (product.Extras is null &&
                    extrasTemplates.TryGetValue(product.CategoryId, out var template))
                {
                    product.Extras = template;
                }
            }

            LoadProducts(products);
        }

        public static async Task LoadInventoryStore(IServiceProvider serviceProvider)
        {
            var inventoryStore = serviceProvider.GetRequiredService<IInventoryStore>();
            await inventoryStore.InitializeAsync();
        }
        public void LoadProducts(IEnumerable<Product> products)
        {

            App.Current.Dispatcher.Invoke(() =>
            {
                Products.Clear();
                _trackers.Clear();

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
        #endregion

        public async Task Register(IEnumerable<Product> transactions)
        {
            // FIXME: SLOW TIME COMPLEXITY - RESOLVE LATER

            var tasks = transactions.Select(item =>
                _productService.EditProperty(item.Id, entity => entity.Qty = item.Qty)
            );  // FIXME: resolve with batch edit (EditRangeProperty)

            await Task.WhenAll(tasks);

            var products = await _productService.GetAll();

            LoadProducts(products);
        }

        public void RemoveProduct(IEnumerable<ProductDataViewModel> products)
        {
            //var productsMap = products.ToHashSet();
            var productsList = products.ToList();
            var product = productsList.First();

            var relativeInventory = ProductsCollectionView.Cast<ProductDataViewModel>()
                .AsParallel()
                .AsOrdered()
                .OrderBy(i => i.Name)
                .Where(i => i.CategoryId == products.First().CategoryId);

            RemoveProductEvent?.Invoke(this, productsList);

            var index = GetIndexByProduct(product, relativeInventory);
            RunFilterSuspended(() => LastProductChanged = (product.CategoryId, new ChangedProductInfo(index, productsList.Select(x => x.Item.Id).ToArray())));

            foreach (var item in productsList)
            {
                var productVm = _trackers.FirstOrDefault(t => t.Key.Item.Id == item.Item.Id).Key;
                _trackers.Remove(productVm);
                Products.Remove(productVm);
            }
        }

        public IEnumerable<ProductDataViewModel> AddProduct(IEnumerable<Product> products)
        {
            var displayProducts = products.Select(product => _productsViewModelFactory(product));


            foreach (var item in displayProducts)
            {
                _trackers[item] = TrackProduct(item);
                Products.Add(item);
            }


            ProductDataViewModel productVm = displayProducts.FirstOrDefault();
            AddProductEvent?.Invoke(this, displayProducts);

            var index = GetIndexByProduct(productVm);
            RunFilterSuspended(() => LastProductChanged = (productVm.Item.CategoryId, new ChangedProductInfo(index, products.Select(x => x.Id).ToArray())));

            return displayProducts;
        }

        public ProductDataViewModel GetProductById(int id)
        {
            var map = Products.ToDictionary(p => p.Item.Id, p => p);
            return map[id];
        }

        public ProductDataViewModel GetProductByIndex(int index)
        {
            return RunFilterSuspended(() => ProductsCollectionView.Cast<ProductDataViewModel>().ElementAt(index));
        }


        public int GetIndexByProduct(ProductDataViewModel product, IEnumerable<ProductDataViewModel> collection = null)
        {
            var map = RunFilterSuspended(() => (collection ?? ProductsCollectionView
                .Cast<ProductDataViewModel>())
                .Select((p, idx) => new { p.Item.Id, Index = idx })
                .ToDictionary(x => x.Id, x => x.Index));  // TODO consider to load this once and keep it updated with changes

            if (map.TryGetValue(product.Item.Id, out var index))
            {
                return index;
            }

            return -1;
        }


        public ProductDataViewModel GetProductByBarcode(string obj)
        {
            return Products.FirstOrDefault(p => p.Barcode == obj);
        }

        #region "Helpers"

        private T RunFilterSuspended<T>(Func<T> action)
        {
            var prevFilter = ProductsCollectionView.Filter;
            ProductsCollectionView.Filter = null;

            try
            {
                return action();
            }
            finally
            {
                ProductsCollectionView.Filter = prevFilter;
            }
        }

        private PropertyChangeTracker<ProductDataViewModel> TrackProduct(ProductDataViewModel viewModel)
        {
            var _tracker = new PropertyChangeTracker<ProductDataViewModel>(viewModel);
            Action<PropertyChangeTracker<ProductDataViewModel>, TargetChangedEventArgs, object, object> method;


            // Track changes to properties and execute commands on change
            // * Commons
            method = (tracker, args, oldValue, newValue) =>
            {
                void handlePropChange() => HandlePropertyChanged(tracker, args, (vm, product, index) =>
                {
                    PropertyChanged?.Invoke(vm, new InventoryStoreEventArgs()
                    {
                        ProductId = product.Id,
                    });

                    LastProductChanged = (product.CategoryId, new ChangedProductInfo(index, new[] { product.Id }));
                });

                _undoRedoManager.Execute(new ProductVMCommandCommonProp(
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
                .Track(nameof(ProductDataViewModel.RetailPrice), viewModel.RetailPrice, method)
                .Track(nameof(ProductDataViewModel.Barcode), viewModel.Barcode, method)
                .Track(nameof(ProductDataViewModel.Expiry), viewModel.Expiry, method)
                .Track(nameof(ProductDataViewModel.Batch), viewModel.Batch, method);

            // * Nested
            method = (tracker, args, oldValue, newValue) =>
            {
                void handlePropChange() => HandlePropertyChanged(tracker, args, (vm, product, index) =>
                {
                    PropertyChanged?.Invoke(vm, new InventoryStoreEventArgs()
                    {
                        ProductId = product.Id,
                    });
                    LastProductChanged = (product.CategoryId, new ChangedProductInfo(index, new[] { product.Id }));
                });

                var propertyChangedArgs = args.Target as IBasePropertyChangeTrackerArgs<BaseViewModel>;
                _undoRedoManager.Execute(new ProductVMCommandNestedProp(
                    propertyChangedArgs.Owner,
                    propertyChangedArgs.Navigator,
                    args.PropertyOf,
                    oldValue,
                    newValue,
                    UpdateProductCommand,
                    handlePropChange,
                    currentViewIn: _navigationViewModelFactory.GetViewByViewModel(_navigationService.CurrentViewModel),
                    targetProduct: viewModel
                ));
            };
            if (viewModel.Extras != null)
                foreach (var item in viewModel.Extras)
                {
                    _tracker.Track(item.Key, nameof(item.Value.Value), item.Value, item.Value.Value, method);
                }


            // * EditPurchase
            method = (tracker, args, oldValue, newValue) =>
            {
                void handlePropChange() => HandlePropertyChanged(tracker, args, async (vm, product, index) =>
                {
                    await App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        vm.Item = product;
                        PurchaseEvent?.Invoke(vm, new InventoryStoreEventArgs()
                        {
                            ProductId = product.Id,
                            Product = vm
                        });
                    }), System.Windows.Threading.DispatcherPriority.Input);

                    LastProductChanged = (product.CategoryId, new ChangedProductInfo(index, new[] { product.Id }));
                });

                _undoRedoManager.Execute(new ProductVMCommandPurchase(
                    viewModel,
                    args.PropertyOf,
                    oldValue,
                    newValue,
                    command: new PurchaseProductCommand(_transactionService, this),
                    propertyChangeHandler: handlePropChange,
                    currentViewIn: typeof(InventoryView)
                ));
            };
            _tracker
                .Track(nameof(ProductDataViewModel.PurchaseDefaultEdit), viewModel.PurchaseDefaultEdit, method)
                .Track(nameof(ProductDataViewModel.PurchaseNormalEdit), viewModel.PurchaseNormalEdit, method);

            return _tracker;
        }

        public PropertyChangeTracker<ProductDataViewModel> GetTrackerByProduct(ProductDataViewModel product)
        {
            return _trackers[product];
        }

        public void PurchaseProduct(ProductDataViewModel viewModel, TargetChangedEventArgs args, object oldValue, object newValue, PurchaseProductCommand purchaseProductCommand, PropertyChangeTracker<ProductDataViewModel> tracker = null)
        {

        }

        private async void HandlePropertyChanged(
            object sender,
            TargetChangedEventArgs e,
            Action<ProductDataViewModel, Product, int> propChanged = null)
        {
            var tracker = (PropertyChangeTracker<ProductDataViewModel>)sender;
            var target = tracker.Target;
            var propertyOf = e.PropertyOf;

            if (tracker.PreviousValues.TryGetValue(propertyOf, out var previousValue) && previousValue is null)
                return;

            int index = Products.IndexOf(Products.FirstOrDefault(x => x.Item.Id == target.Item.Id)); // TODO can source the index from the global map of indecies

            if (index >= 0)
            {
                var product = await _productService.Get(target.Item.Id);

                propChanged?.Invoke(Products[index], product, index);
            }
        }

        #endregion

    }

    public class InventoryStoreEventArgs
    {
        public int ProductId { get; set; }
        public ProductDataViewModel Product { get; set; }
    }
}


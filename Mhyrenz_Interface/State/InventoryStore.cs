using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Mhyrenz_Interface.State
{
    public class InventoryStore : IInventoryStore
    {
        private readonly UndoRedoManager _undoRedoManager;
        private readonly IViewModelFactory<ProductDataViewModel> _productsViewModelFactory;
        private readonly IProductService _productService;
        private readonly ITransactionsService _transactionService;

        private List<PropertyChangeTracker<ProductDataViewModel>> _trackers = new List<PropertyChangeTracker<ProductDataViewModel>>();

        public ObservableCollection<ProductDataViewModel> Products { get; } = new ObservableCollection<ProductDataViewModel>();
        public ICollectionView ProductsCollectionView { get; set; }
        public ICommand UpdateProductCommand { get; set; }
        public ICommand PurchaseProductCommand { get; set; }
        public InventoryStore(
            UndoRedoManager undoRedoManager,
            IEnumerable<Product> products,
            IViewModelFactory<ProductDataViewModel> productsViewModelFactory,
            IProductService productService,
            ITransactionsService transactionsService)
        {
            _undoRedoManager = undoRedoManager;
            _productsViewModelFactory = productsViewModelFactory;
            _productService = productService;
            _transactionService = transactionsService;

            UpdateProductCommand = new UpdateProductCommand(_productService, this);
            PurchaseProductCommand = new PurchaseProductCommand(_transactionService, this);
        }

        public void LoadProducts(IEnumerable<Product> products)
        {
            Products.Clear();
            ChangeTracking.IsInventoryLoaded = true;

            var displayProducts = products.Select(product => _productsViewModelFactory.CreateViewModel(product));

            foreach (var item in displayProducts)
            {
                _trackers.Add(TrackProducts(item));
                Products.Add(item);
            }

            ProductsCollectionView = CollectionViewSource.GetDefaultView(Products);
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
            method = (tracker, args, oldValue, newValue) =>
            {
                _undoRedoManager.Execute(new ProductVMCommandPurchase(
                    viewModel,
                    args.PropertyOf,
                    oldValue,
                    newValue,
                    PurchaseProductCommand
                ));

                HandlePropertyChanged(tracker, args, (vm, product, index) =>
                {

                    Products[index] = _productsViewModelFactory.CreateViewModel(product);

                    PurchaseEvent?.Invoke(vm, new InventoryStoreEventArgs()
                    {
                        ProductId = product.Id,
                        Product = product
                    });
                });
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

                _trackers.Remove(tracker);

                propChanged?.Invoke(Products[index], product, index);

                _trackers.Add(TrackProducts(Products[index]));
            }
        }

        public event EventHandler<InventoryStoreEventArgs> PropertyChanged;
        public event EventHandler<InventoryStoreEventArgs> PurchaseEvent;
    }

    public class InventoryStoreEventArgs
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}


using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Mhyrenz_Interface.State
{
    public class InventoryStore : IInventroyStore
    {
        private readonly UndoRedoManager _undoRedoManager;
        private readonly IViewModelFactory<ProductViewModel> _productsViewModelFactory;
        private readonly IProductService _productService;
        private readonly ITransactionsService _transactionService;
        private readonly TrackerManager<ProductViewModel> _trackerManager;

        public ObservableCollection<ProductViewModel> Products { get; } = new ObservableCollection<ProductViewModel>();
        public ICommand UpdateProductCommand { get; set; }
        public ICommand PurchaseProductCommand { get; set; }
        public InventoryStore(
            UndoRedoManager undoRedoManager,
            IEnumerable<Product> products,
            IViewModelFactory<ProductViewModel> productsViewModelFactory,
            IProductService productService,
            ITransactionsService transactionsService)
        {
            _undoRedoManager = undoRedoManager;
            _productsViewModelFactory = productsViewModelFactory;
            _productService = productService;
            _transactionService = transactionsService;
            _trackerManager = new TrackerManager<ProductViewModel>();

            UpdateProductCommand = new UpdateProductCommand(_productService, this);
            PurchaseProductCommand = new PurchaseProductCommand(_transactionService, this);
        }

        public void LoadProducts(IEnumerable<Product> products)
        {
            Products.Clear();
            ChangeTracking.IsInventoryLoaded = true;

            foreach (var product in products)
            {
                var viewModel = _productsViewModelFactory.CreateViewModel(product);

                TrackProducts(viewModel);

                Products.Add(viewModel);
            }
        }

        private void TrackProducts(ProductViewModel viewModel)
        {

            // Track changes to properties and execute commands on change
            // * Commons
            var commons = new PropertyChangeTracker<ProductViewModel>(viewModel, (propertyName, oldValue, newValue) =>
            {
                _undoRedoManager.Execute(new ProductVMCommandCommon(
                    viewModel,
                    propertyName,
                    oldValue,
                    newValue,
                    UpdateProductCommand
                ));
            })
                .Track(nameof(ProductViewModel.RetailPrice), viewModel.RetailPrice)
                .Track(nameof(ProductViewModel.ListPrice), viewModel.ListPrice)
                .Track(nameof(ProductViewModel.Qty), viewModel.Qty)
                .Track(nameof(ProductViewModel.Name), viewModel.Name)
                .Track(nameof(ProductViewModel.Barcode), viewModel.Barcode)
                .Track(nameof(ProductViewModel.Expiry), viewModel.Expiry)
                .Track(nameof(ProductViewModel.Batch), viewModel.Batch)
                .Track(nameof(ProductViewModel.CategoryId), viewModel.CategoryId);

            // * EditPurchase
            var purchase = new PropertyChangeTracker<ProductViewModel>(viewModel, (propertyName, oldValue, newValue) => {
                _undoRedoManager.Execute(new ProductVMCommandPurchase(
                    viewModel,
                    propertyName,
                    oldValue,
                    newValue,
                    PurchaseProductCommand
                ));
            })
                .Track(nameof(ProductViewModel.PurchaseDefaultEdit), viewModel.PurchaseDefaultEdit)
                .Track(nameof(ProductViewModel.PurchaseNormalEdit), viewModel.PurchaseNormalEdit);

            _trackerManager.Track(
                viewModel,
                (commons, HandleCommonsChanged),
                (purchase, HandlePurchaseChanged));
        }

        private async void HandlePurchaseChanged(object sender, TargetChangedEventArgs e)
        {
            await HandlePropertyChanged(sender, e, async (index, target, tracker) =>
            {
                var product = await _productService.Get(target.Item.Id);
                _trackerManager.Untrack(target);
                Products[index] = _productsViewModelFactory.CreateViewModel(product);
                TrackProducts(Products[index]);
            });
        }

        private async Task HandlePropertyChanged(
            object sender,
            TargetChangedEventArgs e,
            Func<int, ProductViewModel, PropertyChangeTracker<ProductViewModel>, Task> action)
        {
            var tracker = (PropertyChangeTracker<ProductViewModel>)sender;
            var target = (ProductViewModel)e.Target;
            var propertyOf = e.PropertyOf;

            if (!!!tracker.PreviousValues.TryGetValue(propertyOf, out var previousValue) && previousValue is null)
                return;

            int index = Products.IndexOf(Products.FirstOrDefault(x => ReferenceEquals(x, target)));

            if (index >= 0)
               await action(index, target, tracker);

        }

        private async void HandleCommonsChanged(object sender, TargetChangedEventArgs e)
        {
            await HandlePropertyChanged(sender, e, async (index, target, _) =>
            {
                var product = await _productService.Get(target.Item.Id);
                _trackerManager.Untrack(target);
                Products[index] = _productsViewModelFactory.CreateViewModel(product);
                TrackProducts(Products[index]);
            });
        }
    }
}


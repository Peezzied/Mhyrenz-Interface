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
        private readonly IViewModelFactory<ProductDataViewModel> _productsViewModelFactory;
        private readonly IProductService _productService;
        private readonly ITransactionsService _transactionService;
        private readonly TrackerManager<ProductDataViewModel> _trackerManager;

        public ObservableCollection<ProductDataViewModel> Products { get; } = new ObservableCollection<ProductDataViewModel>();
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
            _trackerManager = new TrackerManager<ProductDataViewModel>();

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

        private void TrackProducts(ProductDataViewModel viewModel)
        {

            // Track changes to properties and execute commands on change
            // * Commons
            var commons = new PropertyChangeTracker<ProductDataViewModel>(viewModel, (propertyName, oldValue, newValue) =>
            {
                _undoRedoManager.Execute(new ProductVMCommandCommon(
                    viewModel,
                    propertyName,
                    oldValue,
                    newValue,
                    UpdateProductCommand
                ));
            })
                .Track(nameof(ProductDataViewModel.RetailPrice), viewModel.RetailPrice)
                .Track(nameof(ProductDataViewModel.ListPrice), viewModel.ListPrice)
                .Track(nameof(ProductDataViewModel.Qty), viewModel.Qty)
                .Track(nameof(ProductDataViewModel.Name), viewModel.Name)
                .Track(nameof(ProductDataViewModel.Barcode), viewModel.Barcode)
                .Track(nameof(ProductDataViewModel.Expiry), viewModel.Expiry)
                .Track(nameof(ProductDataViewModel.Batch), viewModel.Batch)
                .Track(nameof(ProductDataViewModel.CategoryId), viewModel.CategoryId);

            // * EditPurchase
            var purchase = new PropertyChangeTracker<ProductDataViewModel>(viewModel, (propertyName, oldValue, newValue) => {
                _undoRedoManager.Execute(new ProductVMCommandPurchase(
                    viewModel,
                    propertyName,
                    oldValue,
                    newValue,
                    PurchaseProductCommand
                ));
            })
                .Track(nameof(ProductDataViewModel.PurchaseDefaultEdit), viewModel.PurchaseDefaultEdit)
                .Track(nameof(ProductDataViewModel.PurchaseNormalEdit), viewModel.PurchaseNormalEdit);

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
            Func<int, ProductDataViewModel, PropertyChangeTracker<ProductDataViewModel>, Task> action)
        {
            var tracker = (PropertyChangeTracker<ProductDataViewModel>)sender;
            var target = (ProductDataViewModel)e.Target;
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


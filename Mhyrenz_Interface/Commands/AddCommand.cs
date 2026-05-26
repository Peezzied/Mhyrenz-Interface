using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HandyControl.Controls;
using HandyControl.Data;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Mhyrenz_Interface.Views;

namespace Mhyrenz_Interface.Commands
{
    public class AddCommand : BaseAsyncCommand, IUndoRedoBound
    {
        private readonly CreateCommand<DeleteCommand> _deleteCommand;
        private readonly AddProductViewModel _viewModel;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;
        private bool CanSubmit = true;
        private IEnumerable<ProductDataViewModel> _products;

        public AddCommand(AddProductViewModel vm, IProductService productService, IInventoryStore inventoryStore, IUndoRedoManager undoRedoManager, CreateCommand<DeleteCommand> deleteCommand)
        {
            _viewModel = vm;
            _deleteCommand = deleteCommand;
            _undoRedoManager = undoRedoManager;
            _productService = productService;
            _inventoryStore = inventoryStore;
        }

        public bool AllowBack { get; private set; } = true;

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter)
                && Validator.TryValidateObject(_viewModel, new ValidationContext(_viewModel), null, validateAllProperties: true)
                && CanSubmit;
        }

        private void SideEffect(NavigationViewModel vm)
        {
            var view = vm as InventoryViewModel;

            view.RowIntoView(_inventoryStore.LastProductChanged.Category, _inventoryStore.LastProductChanged.ChangedProductInfo.Value.Products);
        }

        public override void Execute(object parameter)
        {
            _undoRedoManager.Push(new UndoRedoBoundCommand(this, SideEffect, typeof(InventoryView), parameter));
            base.Execute(parameter);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            CanSubmit = false;
            var product = await _productService.Create(new Product
            {
                Name = _viewModel.Name,
                RetailPrice = _viewModel.Price,
                Qty = _viewModel.Qty,
                CategoryId = _viewModel.SelectedCategory.Id,
                Expiry = _viewModel.Expiry,
                Batch = _viewModel.Batch,
                Barcode = _viewModel.Barcode
            });

            _products = _inventoryStore.AddProduct(new[] { await _productService.Get(product.Id) });

            Growl.Success(new GrowlInfo
            {
                Message = $"Product \"{product.Name}\" has been added successfully!",
                ShowDateTime = false,
            });
            _viewModel.RaiseSubmitSuccess(_products.First());
        }

        public void ExecuteRaw(object parameter)
        {
            base.Execute(parameter);
        }

        public async void Redo(object parameter = null)
        {
            var product = (await _productService.RemoveManyBack(_products.Select(i => i.Item.Id))).First();

            _products = _inventoryStore.AddProduct(new[] { product });

            Growl.Success(new GrowlInfo
            {
                Message = $"Product \"{product.Name}\" has been added successfully!",
                ShowDateTime = false,
            });

            _viewModel.RaiseSubmitSuccess(_products.First());
        }

        public void Undo(object parameter = null)
        {
            var deleteCmd = _deleteCommand();

            deleteCmd.ExecuteRaw(_products);

            AllowBack = deleteCmd.AllowBack;
        }
    }
}

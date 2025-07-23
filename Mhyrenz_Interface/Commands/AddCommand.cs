using HandyControl.Controls;
using HandyControl.Data;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.Commands
{
    public class AddCommand : BaseAsyncCommand, IUndoRedoBound
    {
        private readonly AddProductViewModel _viewModel;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;
        private bool CanSubmit = true;
        private ProductDataViewModel _product;

        public AddCommand(AddProductViewModel vm, IProductService productService, IInventoryStore inventoryStore, IUndoRedoManager undoRedoManager)
        {
            _viewModel = vm;
            _undoRedoManager = undoRedoManager;
            _productService = productService;
            _inventoryStore = inventoryStore;
        }

        public Type CurrentViewIn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) 
                && Validator.TryValidateObject(_viewModel, new ValidationContext(_viewModel), null, validateAllProperties: true)
                && CanSubmit;
        }

        public override void Execute(object parameter)
        {
            _undoRedoManager.Push(new UndoRedoBoundCommand(this, typeof(InventoryView), parameter));

            base.Execute(parameter);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            CanSubmit = false;
            var product = await _productService.Add(new Product
            {
                Name = _viewModel.Name,
                RetailPrice = (decimal)_viewModel.Price, // RESOLVE DECIMAL TO DOUBLE
                Qty = _viewModel.Qty,
                CategoryId = _viewModel.SelectedCategory.Id,
                Expiry = _viewModel.Expiry,
                Batch = _viewModel.Batch,
            });

            _product = _inventoryStore.AddProduct(await _productService.Get(product.Id));

            Growl.Success(new GrowlInfo
            {
                Message = $"Product \"{product.Name}\" has been added successfully!" ,
                ShowDateTime = false,
            });
            _viewModel.RaiseSubmitSuccess(_product);
        }

        public void ExecuteRaw(object parameter)
        {
            base.Execute(parameter);
        }

        public void Redo(object parameter = null)
        {
            ExecuteRaw(parameter);
        }

        public void Undo(object parameter = null)
        {
            var deleteCmd = new DeleteCommand(_productService, _inventoryStore, _undoRedoManager);

            void removeItems()
            {
                _inventoryStore.RemoveProduct(_product);
            }

            deleteCmd.ExecuteRaw(new InventoryDataGridVmDTO
            {
                ProductData = new[] { _product },
                RemoveItemsHandler = removeItems
            });

            //await _productService.Remove(_product.Item);

            //_inventoryStore.RemoveProduct(_product);

            //Growl.Success(new GrowlInfo
            //{
            //    Message = $"Removed product \"{_product.Name}\" successfully.",
            //    ShowDateTime = false,
            //});
            //_viewModel.RaiseRowIntoView(_product);
        }
    }

    public interface IUndoRedoBound: ICommandAsync, ICommand
    {
        void Undo(object parameter);
        void Redo(object parameter);
        void ExecuteRaw(object parameter);
    }
}

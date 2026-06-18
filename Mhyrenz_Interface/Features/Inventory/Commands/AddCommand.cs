using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HandyControl.Controls;
using HandyControl.Data;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Features.Inventory.Views;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Inventory.Commands
{
    [Obsolete]
    public class AddCommand : UndoRedoBoundCommand
    {
        private readonly CreateCommand<DeleteCommand> _deleteCommand;
        private readonly AddProductViewModel _viewModel;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;
        private bool CanSubmit = true;
        private IEnumerable<ProductDataViewModel> _products;
        private ProductVMRowInfo _rowInfo;

        public AddCommand(AddProductViewModel vm, IProductService productService, IInventoryStore inventoryStore, IUndoRedoManager undoRedoManager, CreateCommand<DeleteCommand> deleteCommand)
            : base(typeof(InventoryView))
        {
            _viewModel = vm;
            _deleteCommand = deleteCommand;
            _undoRedoManager = undoRedoManager;
            _productService = productService;
            _inventoryStore = inventoryStore;
        }

        public bool AllowBack { get; private set; } = true;

        //public override bool CanExecute(object parameter)
        //{
        //    return base.CanExecute(parameter)
        //        && Validator.TryValidateObject(_viewModel, new ValidationContext(_viewModel), null, validateAllProperties: true)
        //        && CanSubmit;
        //}

        private async Task SideEffect(NavigationViewModel vm)
        {
            var view = vm as InventoryViewModel;
            view.RowIntoView(_rowInfo.Category, _rowInfo.Products);
        }

        public override async Task Command()
        {
            CanSubmit = false;
            var productVm = await _productService.Create(new Product
            {
                Name = _viewModel.Name,
                RetailPrice = _viewModel.Price,
                Qty = _viewModel.Qty,
                CategoryId = _viewModel.SelectedCategory.Id,
                Expiry = _viewModel.Expiry,
                Batch = _viewModel.Batch,
                Barcode = _viewModel.Barcode
            });

            var product = await _productService.Get(productVm.Id);
            _products = _inventoryStore.AddProduct(new[] { product });
            _rowInfo = new ProductVMRowInfo
            {
                Category = product.CategoryId,
                Products = new int[] { product.Id }
            };

            Growl.Success(new GrowlInfo
            {
                Message = $"Product \"{product.Name}\" has been added successfully!",
                ShowDateTime = false,
            });
            _viewModel.RaiseSubmitSuccess(_products.First());
        }

        //public override async Task Redo(UndoRedoInfo info)
        //{
        //    // TODO utilize ExecuteRaw. Use the parameter
        //    var product = (await _productService.RemoveManyBack(_products.Select(i => i.Item.Id))).First();

        //    _products = _inventoryStore.AddProduct(new[] { product });
        //    _rowInfo = new ProductVMRowInfo
        //    {
        //        Category = product.CategoryId,
        //        Products = new int[] { product.Id }
        //    };

        //    Growl.Success(new GrowlInfo
        //    {
        //        Message = $"Product \"{product.Name}\" has been added successfully!",
        //        ShowDateTime = false,
        //    });

        //    _viewModel.RaiseSubmitSuccess(_products.First());
        //}

        //public override async Task Undo(UndoRedoInfo info)
        //{
        //    var deleteCmd = _deleteCommand();

            
        //}
    }
}

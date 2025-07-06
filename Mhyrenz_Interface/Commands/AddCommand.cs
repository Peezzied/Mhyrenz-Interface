using HandyControl.Controls;
using HandyControl.Data;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Commands
{
    public class AddCommand : BaseAsyncCommand
    {
        private readonly AddProductViewModel _validationContext;
        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;
        private bool CanSubmit = true;

        public AddCommand(AddProductViewModel validationContext, IProductService productService, IInventoryStore inventoryStore)
        {
            _validationContext = validationContext;
            _productService = productService;
            _inventoryStore = inventoryStore;
        }

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) 
                && Validator.TryValidateObject(_validationContext, new ValidationContext(_validationContext), null)
                && CanSubmit;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            CanSubmit = false;
            var product = await _productService.Add(new Domain.Models.Product
            {
                Name = _validationContext.Name,
                RetailPrice = (decimal)_validationContext.Price, // RESOLVE DECIMAL TO DOUBLE
                Qty = _validationContext.Qty,
                CategoryId = _validationContext.SelectedCategory.Id,
                Expiry = _validationContext.Expiry,
                Batch = _validationContext.Batch,
            });

            var result = _inventoryStore.AddProduct(await _productService.Get(product.Id));

            Growl.Success(new GrowlInfo
            {
                Message = $"Product {product.Name} has been added successfully!" ,
                ShowDateTime = false,
            });
            _validationContext.RaiseSubmitSuccess(result);
        }
    }
}

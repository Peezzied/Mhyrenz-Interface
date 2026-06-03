using System;
using System.Windows.Input;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.Commands
{
    public class ProductVMCommandCommonProp : ProductVMPropertyChangeCommand
    {
        private readonly DTO _dto;
        private readonly IProductService _productService;
        private readonly IProductService productService;

        public ProductVMCommandCommonProp(DTO dto, IProductService productService) : base(dto)
        {
            _dto = dto;
            _productService = productService;
        }

        public override async void Command(object parameter, ActionType intent)
        {
            await _productService.Update(_dto.Product.Id, _dto.Product);
        }

        public new class DTO : PropertyChangeCommand<ProductVMRowInfo>.DTO
        {
            public Product Product { get; set; }
        }
    }
}

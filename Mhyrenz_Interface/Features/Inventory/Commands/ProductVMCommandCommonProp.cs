using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Navigation;

namespace Mhyrenz_Interface.Features.Inventory.Commands
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

        protected override async Task CompleterHandler(NavigationViewModel vm)
        {
            await base.CompleterHandler(vm);
            await Complete();
        }

        private async Task Complete()
        {
            await _productService.Update(_dto.Product.Id, _dto.Product);
        }

        public override async Task Command()
        {
            await base.Command();

            if (Intent == ActionType.Normal)
            {
                await Complete();
            }
        }

        public new class DTO : PropertyChangeCommand<ProductVMRowInfo>.DTO
        {
            public Product Product { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Inventory.Commands
{
    public class ProductVMCommandMarkupRate : ProductVMPropertyChangeCommand
    {
        private readonly DTO _dto;
        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;

        public ProductVMCommandMarkupRate(DTO dto, IProductService productService, IInventoryStore inventoryStore) : base(dto)
        {
            _dto = dto;
            _productService = productService;
            _inventoryStore = inventoryStore;
        }

        protected override async Task CompleterHandler(NavigationViewModel vm)
        {
            await base.CompleterHandler(vm);
            await Complete();
        }
        private async Task Complete()
        {
            var product = await _productService.SetMarkupRate(_dto.ProductId, (decimal)(Intent != ActionType.Undo
                ? PropertyChangedArgs.NewValue
                : PropertyChangedArgs.OldValue));

            if (_inventoryStore.Store.TryGetValue(_dto.ProductId, out var vm))
            {
                vm.Item = product;
                await vm.RequestFlash(Shared.Behaviors.DataGridFlashBehavior.OperationType.Update);
            }
        }

        public override async Task Command()
        {
            await base.Command();

            if (Intent == ActionType.Normal)
            {
                await Complete();
            }
        }

        public new class DTO: ProductVMPropertyChangeCommand.DTO
        {
            public int ProductId { get; set; }
        }
    }
}

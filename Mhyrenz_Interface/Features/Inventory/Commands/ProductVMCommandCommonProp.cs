using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Inventory.Commands
{
    public class ProductVMCommandCommonProp : ProductVMPropertyChangeCommand
    {
        private readonly DTO _dto;
        private readonly IInventoryStore _inventoryStore;
        private readonly IProductService _productService;
        private readonly IProductService productService;

        public ProductVMCommandCommonProp(DTO dto, IInventoryStore inventoryStore, IProductService productService) : base(dto)
        {
            _dto = dto;
            _inventoryStore = inventoryStore;
            _productService = productService;
        }

        protected override async Task CompleterHandler(BaseViewModel vm)
        {
            await base.CompleterHandler(vm);

            if (vm is InventoryViewModel inventory
                && inventory.InventoryDataGrid is IFlashRequestable flasher
                && _inventoryStore.Store.TryGetValue(_dto.Product.Id, out var product))
            {
                await flasher.RequestFlash(product, DataGridFlashBehavior.OperationType.Update);
            }
            else
            {
                Cancel = true;
            }
        }

        public override async Task Command()
        {
            await base.Command();

            await _productService.Update(_dto.Product.Id, _dto.Updater ?? (p =>
            {
                p.GetType().GetProperty(_dto.PropertyName).SetValue(p,
                    Intent != ActionType.Undo ? PropertyChangedArgs.NewValue : PropertyChangedArgs.OldValue);
            }));
        }

        public new class DTO : PropertyChangeCommand<ProductVMRowInfo>.DTO
        {
            public Product Product { get; set; }
            public string PropertyName { get; set; }
            public Action<Product> Updater { get; internal set; }
        }
    }
}

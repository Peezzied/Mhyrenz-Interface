using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Features.Inventory.Views;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface.Features.Inventory.Commands
{
    public class DeleteCommand : UndoRedoBoundCommand
    {
        private readonly int _categoryId;
        private readonly IEnumerable<int> _productIds;
        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;
        private readonly IUndoRedoManager _undoRedoManager;
        private ProductVMRowInfo _rowInfo;

        public DeleteCommand(int categoryId, IEnumerable<int> productIds, IProductService productService, IInventoryStore inventoryStore, IUndoRedoManager undoRedoManager) : base(typeof(InventoryView))
        {
            _categoryId = categoryId;
            _productIds = productIds.ToList();
            _productService = productService;
            _inventoryStore = inventoryStore;
            _undoRedoManager = undoRedoManager;

            Completer = CompleterHandler;
        }

        public override async Task Command()
        {
            if (Intent != ActionType.Undo)
            {
                var prompt = MessageBox.Show($"Do you really want to remove {_productIds.Count()} items?", "Remove Action", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (prompt == MessageBoxResult.No)
                {
                    Cancel = true;
                    return;
                }
            }

            _rowInfo = new ProductVMRowInfo
            {
                Category = _categoryId,
                Products = _productIds.ToArray()
            };

            if (Intent == ActionType.Undo)
            {
                var products = await _productService.RemoveManyBack(_productIds);
                _inventoryStore.AddProduct(products.ToList());
                return;
            }

            await _productService.RemoveMany(_productIds);
        }

        private async Task CompleterHandler(BaseViewModel vm)
        {
            if (vm is InventoryViewModel inventory
                && inventory.InventoryDataGrid is IFlashRequestable flasher
                && vm is IDataGridTabHost host)
            {
                host.RowIntoView(_rowInfo);

                if (Intent != ActionType.Undo)
                {
                    await Task.WhenAll(_productIds.Select(p => flasher.RequestFlash(_inventoryStore.Store[p], DataGridFlashBehavior.OperationType.Remove)));
                    _inventoryStore.Store.RemoveMany(_productIds);
                }
                else
                {
                    await Task.WhenAll(_productIds.Select(p => flasher.RequestFlash(_inventoryStore.Store[p], DataGridFlashBehavior.OperationType.New)));
                }
            }
            else
            {
                Cancel = true;
            }
        }
    }
}

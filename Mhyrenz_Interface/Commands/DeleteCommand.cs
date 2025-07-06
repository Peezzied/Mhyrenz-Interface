using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mhyrenz_Interface.Commands
{
    public class DeleteCommand : BaseAsyncCommand
    {
        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;

        public DeleteCommand(IProductService productService)
        {
            _productService = productService;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            if (parameter is InventoryDataGridVmDTO dto)
            {
                var items = dto.ProductData;
                var prompt = MessageBox.Show($"Do you really want to remove {items.Count()} items?", "Remove Action", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (prompt == MessageBoxResult.No)
                {
                    if (dto.DeleteHandle == null)
                        return;

                    dto.DeleteHandle();
                    return;
                }

                await _productService.RemoveMany(items.Select(i => i.Item));
                if (dto.DeleteHandle == null)
                    dto.RemoveItems();


            }
        }
    }
}

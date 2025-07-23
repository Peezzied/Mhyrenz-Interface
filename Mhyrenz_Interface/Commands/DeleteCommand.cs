using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.Views;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mhyrenz_Interface.Commands
{
    public class DeleteCommand : BaseAsyncCommand, IUndoRedoBound
    {
        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;
        private readonly IUndoRedoManager _undoRedoManager;
        private IEnumerable<Product> _products;

        public DeleteCommand(IProductService productService, IInventoryStore inventoryStore, IUndoRedoManager undoRedoManager)
        {
            _productService = productService;
            _inventoryStore = inventoryStore;
            _undoRedoManager = undoRedoManager;
        }

        public override void Execute(object parameter)
        {
            _undoRedoManager.Push(new UndoRedoBoundCommand(this, typeof(InventoryView), parameter));

            base.Execute(parameter);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            if (parameter is InventoryDataGridVmDTO dto)
            {
                _products = dto.ProductData.Select(i => i.Item).ToList();
                var prompt = MessageBox.Show($"Do you really want to remove {_products.Count()} items?", "Remove Action", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (prompt == MessageBoxResult.No)
                    return;

                //_inventoryStore.LastProductChanged = (_inventoryStore.Products.IndexOf(items.First()), items.First());

                await _productService.RemoveMany(_products);
                dto.RemoveItemsHandler();
            }
        }

        public void ExecuteRaw(object parameter)
        {
            base.Execute(parameter);
        }

        public void Redo(object parameter)
        {
            var productsMap = new HashSet<int>(_products.Select(p => p.Id));
            var products = _inventoryStore.Products.Where(p => productsMap.Contains(p.Item.Id));
            ExecuteRaw(new InventoryDataGridVmDTO {
                ProductData = products,
                RemoveItemsHandler = () =>
                {
                    _inventoryStore.RemoveProduct(products);
                }
            });
        }

        public async void Undo(object parameter = null)
        {
            var products = await _productService.AddMany(_products.Select(i => i.Clone()));
            _products = (await _productService.GetAll()).Where(i => products.Any(x => x.Id == i.Id));

            _inventoryStore.AddProduct(_products);
        }
    }
}

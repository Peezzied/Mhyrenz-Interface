using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
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

        public bool AllowBack { get; private set; } = true;

        public DeleteCommand(IProductService productService, IInventoryStore inventoryStore, IUndoRedoManager undoRedoManager)
        {
            _productService = productService;
            _inventoryStore = inventoryStore;
            _undoRedoManager = undoRedoManager;
        }

        private void SideEffect(NavigationViewModel vm)
        {
            var view = vm as InventoryViewModel;

            view.RowIntoView(_inventoryStore.LastProductChanged.Products);
        }

        public override void Execute(object parameter)
        {
            base.Execute(parameter);

            if (_products != null)
                _undoRedoManager.Push(new UndoRedoBoundCommand(this, SideEffect, typeof(InventoryView), parameter));
        }

        public override async Task ExecuteAsync(object parameter)
        {
            AllowBack = true;

            var products = parameter.CastTo<IEnumerable<ProductDataViewModel>>();
            var prompt = MessageBox.Show($"Do you really want to remove {products.Count()} items?", "Remove Action", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (prompt == MessageBoxResult.No)
            {
                AllowBack = false;
                return;
            }


            //var products = parameter.CastTo<IEnumerable<ProductDataViewModel>>();
            _products = products.Select(i => i.Item).ToList();

            await _productService.EditPropertyRange(_products, nameof(Product.IsDeleted), true);
            _inventoryStore.RemoveProduct(products);

        }

        public void ExecuteRaw(object parameter)
        {
            base.Execute(parameter);
        }

        public void Redo(object parameter)
        {
            var productsMap = new HashSet<int>(_products.Select(p => p.Id));
            var products = _inventoryStore.Products.Where(p => productsMap.Contains(p.Item.Id));
            ExecuteRaw(products);
        }

        public async void Undo(object parameter = null)
        {
            _products = await _productService.EditPropertyRange(_products.Select(i => i), nameof(Product.IsDeleted), false);
            _inventoryStore.AddProduct(_products);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Features.Inventory.Views;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Inventory.Commands
{
    public class DeleteCommand : BaseAsyncCommand, IUndoRedoBound
    {
        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;
        private readonly IUndoRedoManager _undoRedoManager;
        private ICollection<int> _products;
        private ProductVMRowInfo _rowInfo;

        public bool AllowBack { get; private set; } = true;

        public DeleteCommand(IProductService productService, IInventoryStore inventoryStore, IUndoRedoManager undoRedoManager)
        {
            _productService = productService;
            _inventoryStore = inventoryStore;
            _undoRedoManager = undoRedoManager;
        }

        private async Task SideEffect(NavigationViewModel vm)
        {
            var view = vm as InventoryViewModel;

            view.RowIntoView(_rowInfo.Category, _rowInfo.Products);
        }

        public override void Execute(object parameter)
        {
            if (_undoRedoManager.CanRedo)
            {
                if (!UndoRedoManager.ShowWarning())
                    return;
            }

            base.Execute(parameter);

            if (_products != null)
                _undoRedoManager.Push(new UndoRedoBoundCommand(this, SideEffect, typeof(InventoryView), parameter));
        }

        public override async Task ExecuteAsync(object parameter)
        {
            AllowBack = true;

            var products = parameter.CastTo<IEnumerable<ProductDataViewModel>>().ToList();
            var prompt = MessageBox.Show($"Do you really want to remove {products.Count()} items?", "Remove Action", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (prompt == MessageBoxResult.No)
            {
                AllowBack = false;
                return;
            }


            //var products = parameter.CastTo<IEnumerable<ProductDataViewModel>>();
            _products = products.Select(i => i.Item.Id).ToList();

            _rowInfo = new ProductVMRowInfo
            {
                Category = products.First().CategoryId,
                Products = _products.ToArray()
            };

            await _productService.RemoveMany(_products); // TODO encapsulate this in inventorystore removeproduct
            _inventoryStore.RemoveProduct(products);


        }

        public void ExecuteRaw(object parameter)
        {
            base.Execute(parameter);
        }

        public void Redo(object parameter)
        {
            var productsMap = _products.ToHashSet();
            var products = _inventoryStore.Store.Where(p => productsMap.Contains(p.Item.Id));
            ExecuteRaw(products);
        }

        public async void Undo(object parameter = null)
        {
            // TODO use the AddCommand once it supports multiple products
            var products = await _productService.RemoveManyBack(_products);

            _inventoryStore.AddProduct(products.ToList());
        }
    }
}

using Mhyrenz_Interface.Domain.Models;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Mhyrenz_Interface.ViewModels
{
    public class InventoryTabItem : BaseViewModel
    {
        private readonly InventoryDataGridViewModel _inventoryDataGridViewModel;

        public string Name => _category;
        public int Id => _categoryId;

        private ContentControl _controlInstance;
        private Predicate<object> _originalFilter;

        public ContentControl ControlInstance
        {
            get
            {
                if (_controlInstance == null)
                {
                    _controlInstance = new ContentControl
                    {
                        Content = _inventoryDataGridViewModel,
                        ContentTemplate = (DataTemplate)App.Current.FindResource("DataGridDetailedLayout")
                    };
                }
                return _controlInstance;
            }
        }

        private readonly ICollectionView _allProducts;
        private readonly string _category;
        private readonly int _categoryId;
        private readonly Func<ProductDataViewModel, bool> _searchFilter;

        public InventoryTabItem(
            InventoryDataGridViewModel inventoryDataGridViewModel,
            Category category,
            ICollectionView allProducts,
            Func<ProductDataViewModel, bool> searchFilter)
        {
            _category = category.Name;
            _categoryId = category.Id;
            _allProducts = allProducts;
            _searchFilter = searchFilter;

            _inventoryDataGridViewModel = inventoryDataGridViewModel; // REFACTOR WITH FACTORY

            // Kick off deferred loading (non-blocking)
            DeferInventoryInitialization();
        }

        private void DeferInventoryInitialization()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _originalFilter = _allProducts.Filter;

                _allProducts.Filter += item =>
                {
                    if (_originalFilter != null && !_originalFilter(item))
                        return false;

                    return _searchFilter(item as ProductDataViewModel);
                };

                _inventoryDataGridViewModel.Inventory = _allProducts;
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        public void Refresh() => _inventoryDataGridViewModel.Inventory?.Refresh();

        public int ProductIndexOf(ProductDataViewModel addedProduct)
        {
            var inventory = _inventoryDataGridViewModel.Inventory.Cast<ProductDataViewModel>();

            int bestMatchIndex = -1;
            var targetId = addedProduct.Item.Id;

            for (int i = 0; i < inventory.Count(); i++)
            {
                int id = inventory.ElementAt(i).Item.Id;

                if (id == targetId)
                {
                    bestMatchIndex = i; // Exact match
                    break;
                }

                if (id < targetId && (bestMatchIndex == -1 || inventory.ElementAt(bestMatchIndex).Item.Id < id))
                {
                    bestMatchIndex = i; // Closest smaller item
                }
            }

            return bestMatchIndex;

        }
    }
}

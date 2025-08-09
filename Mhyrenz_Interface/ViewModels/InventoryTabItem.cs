using Mhyrenz_Interface.Controls;
using Mhyrenz_Interface.Controls.RibbonBarTools;
using Mhyrenz_Interface.Domain.Models;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace Mhyrenz_Interface.ViewModels
{
    public class InventoryTabItem : BaseViewModel
    {
        private readonly InventoryDataGridViewModel _inventoryDataGridViewModel;

        public string Name => _category.Name;
        public int Id => _category.Id;

        private ContentControl _controlInstance;
        private Predicate<object> _originalFilter;

        public ContentControl ControlInstance
        {
            get
            {
                if (_controlInstance == null)
                {
                    RibbonBar = new InventoryDataGridTools() { DataContext = this };

                    CanToggleGenericName = _category.Type == ProductType.GenericNamed;

                    CanToggleDependents = CanToggleGenericName;

                    _controlInstance = new ContentControl
                    {
                        Content = _inventoryDataGridViewModel,
                        ContentTemplate = (DataTemplate)App.Current.FindResource("DataGridDetailedLayout")
                    };
                }
                return _controlInstance;
            }
        }

        private bool _canToggleDependents;
        public bool CanToggleDependents
        {
            get => _canToggleDependents;
            set
            {
                _canToggleDependents = value;
                OnPropertyChanged(nameof(CanToggleDependents));
            }
        }

        private bool _canToggleGenericName;
        public bool CanToggleGenericName
        {
            get => _canToggleGenericName;
            set
            {
                _canToggleGenericName = value;
                OnPropertyChanged(nameof(CanToggleGenericName));
            }
        }

        private readonly ICollectionView _allProducts;
        private readonly Category _category;
        private readonly Func<ProductDataViewModel, bool> _searchFilter;

        public UserControl RibbonBar { get; private set; }

        public InventoryTabItem(
            UserControl ribbonBar,
            InventoryDataGridViewModel inventoryDataGridViewModel,
            Category category,
            ICollectionView allProducts,
            Func<ProductDataViewModel, bool> searchFilter)
        {
            _category = category;
            _allProducts = allProducts;
            _searchFilter = searchFilter;

            RibbonBar = ribbonBar;

            _inventoryDataGridViewModel = inventoryDataGridViewModel; // REFACTOR WITH FACTORY

            // Kick off deferred loading (non-blocking)
            DeferInventoryInitialization();
        }

        public override void Dispose()
        {
            _inventoryDataGridViewModel.Dispose();
        }

        private void DeferInventoryInitialization()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _originalFilter = _allProducts.Filter;

                _allProducts.Filter = item =>
                {
                    if (_originalFilter != null && !_originalFilter(item))
                        return false;

                    return _searchFilter(item as ProductDataViewModel);
                };

                _inventoryDataGridViewModel.Inventory = _allProducts;
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        public void Refresh() => _inventoryDataGridViewModel.Inventory?.Refresh();

        public int ProductIndexOf(ProductDataViewModel product)
        {
            var relativeInventoryMap = _allProducts.Cast<ProductDataViewModel>()
                .Select((p, index) => new { p.Item.Id, Index = index })
                .ToDictionary(i => i.Id, i => i.Index);

            if (relativeInventoryMap.TryGetValue(product.Item.Id, out var value))
            {
                return value;
            }
            return -1;

            //int targetId = product.Item.Id;
            //int closestId = -1;
            //int minDifference = int.MaxValue;

            //foreach (var item in _allProducts.SourceCollection.Cast<ProductDataViewModel>().Where(i => i.CategoryId == Id))
            //{
            //    int currentId = item.Item.Id;
            //    int diff = Math.Abs(currentId - targetId);

            //    if (diff < minDifference)
            //    {
            //        minDifference = diff;
            //        closestId = currentId;
            //    }
            //}

            //    return relativeInventoryMap[closestId]; // -1 if nothing is smaller than target
        }


        public int GetRelativeIndex(ProductDataViewModel product)
        {
            var oldFilter = _inventoryDataGridViewModel.Inventory.Filter;
            _inventoryDataGridViewModel.Inventory.Filter = null;
            var inventory = _inventoryDataGridViewModel.Inventory.Cast<ProductDataViewModel>();

            int bestMatchIndex = -1;
            var targetId = product.Item.Id;

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

            _inventoryDataGridViewModel.Inventory.Filter = oldFilter;

            return bestMatchIndex;

        }
    }
}

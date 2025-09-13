using HandyControl.Controls;
using HandyControl.Tools;
using Mhyrenz_Interface.Controls;
using Mhyrenz_Interface.Controls.RibbonBarTools;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.AppSettingsManager;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

                    _controlInstance = new InventoryDataGrid
                    {
                        Layout = InventoryDataGridLayout.Detailed,
                        DataContext = _inventoryDataGridViewModel,
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
        private readonly AppSettingsManager _appSettingsManager;
        private readonly Category _category;
        private readonly Func<ProductDataViewModel, bool> _searchFilter;

        public UserControl RibbonBar { get; private set; }

        public ICommand ToggleColumnCommand { get; }

        public InventoryTabItem(
            UserControl ribbonBar,
            InventoryDataGridViewModel inventoryDataGridViewModel,
            Category category,
            ICollectionView allProducts,
            IOptions<AppSettingsManager.Settings> appSettings,
            AppSettingsManager appSettingsManager,
            Func<ProductDataViewModel, bool> searchFilter)
        {
            _appSettingsManager = appSettingsManager;
            _category = category;
            _allProducts = allProducts;
            _searchFilter = searchFilter;
            
            RibbonBar = ribbonBar;
            ToggleColumnCommand = new RelayCommand<Columns>(ToggleColumn);

            _inventoryDataGridViewModel = inventoryDataGridViewModel;

            var categorySettings = appSettings.Value.Inventory.First(c => c.Key.ConvertToInt() == Id).Value;
            _inventoryDataGridViewModel.IdColumn = categorySettings.IdColumn;
            _inventoryDataGridViewModel.GenericNameColumn = categorySettings.GenericColumn ?? false;
            _inventoryDataGridViewModel.SupplierColumn = categorySettings.SupplierColumn;
            _inventoryDataGridViewModel.BatchColumn = categorySettings.BatchColumn;
            _inventoryDataGridViewModel.ExpiryColumn = categorySettings.ExpiryDateColumn;

            // Kick off deferred loading (non-blocking)
            DeferInventoryInitialization();
        }

        private void ToggleColumn(Columns column)
        {
            switch (column)
            {
                case Columns.IdColumn:
                    _inventoryDataGridViewModel.IdColumn = !_inventoryDataGridViewModel.IdColumn;
                    UpdateColumnSetting(nameof(InventorySettings.IdColumn), _inventoryDataGridViewModel.IdColumn);
                    break;
                case Columns.GenericNameColumn:
                    _inventoryDataGridViewModel.GenericNameColumn = !_inventoryDataGridViewModel.GenericNameColumn;
                    UpdateColumnSetting(nameof(InventorySettings.GenericColumn), _inventoryDataGridViewModel.GenericNameColumn);
                    break;
                case Columns.BatchColumn:
                    _inventoryDataGridViewModel.BatchColumn = !_inventoryDataGridViewModel.BatchColumn; 
                    UpdateColumnSetting(nameof(InventorySettings.BatchColumn), _inventoryDataGridViewModel.BatchColumn);
                    break;
                case Columns.ExpiryColumn:
                    _inventoryDataGridViewModel.ExpiryColumn = !_inventoryDataGridViewModel.ExpiryColumn;
                    UpdateColumnSetting(nameof(InventorySettings.ExpiryDateColumn), _inventoryDataGridViewModel.ExpiryColumn); 
                    break;
                case Columns.SupplierColumn:
                    _inventoryDataGridViewModel.SupplierColumn = !_inventoryDataGridViewModel.SupplierColumn;
                    UpdateColumnSetting(nameof(InventorySettings.SupplierColumn), _inventoryDataGridViewModel.SupplierColumn); 
                    break;
            }
        }

        private void UpdateColumnSetting(string property, bool value)
        {
            try
            {
                _appSettingsManager.UpdateAppSettingsNode(new[] { nameof(AppSettingsManager.Settings.Inventory), Id.ToString(), property }, value);
            }
            catch (Exception e)
            {
                Growl.Error($"Failed to save settings due to an error: {e.Message}");
                throw;
            }
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

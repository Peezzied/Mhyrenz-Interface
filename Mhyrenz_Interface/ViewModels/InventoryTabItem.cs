using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Mhyrenz_Interface.Controls.Behaviors;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.State;

namespace Mhyrenz_Interface.ViewModels
{
    public class InventoryTabItem : BaseViewModel
    {

        public string Name => _category.Name;
        public Brush Color => _categoryStore.Colors[_category.Id];
        public int Id => _category.Id;

        public ObservableDictionary<string, ColumnSettingViewModel> Columns { get; set; }
        public ObservableCollection<ColumnSettingViewModel> ColumnsView { get; set; }

        private Predicate<object> _originalFilter;

        private InventoryDataGridViewModel _contentViewModel;
        public InventoryDataGridViewModel ContentViewModel
        {
            get
            {
                return _contentViewModel;
            }
            set
            {
                _contentViewModel = value;
                OnPropertyChanged(nameof(ContentViewModel));
            }
        }

        private ICollectionView _view;
        private readonly ICategoryStore _categoryStore;
        private readonly Category _category;
        private readonly Func<ProductDataViewModel, bool> _searchFilter;
        private readonly Predicate<object> _filter;
        private readonly CreateViewModel<ColumnSettingViewModel> _columnSettingViewModelFactory;
        private readonly InventorySettingsProvider _inventorySettingsProvider;
        private readonly InventoryDataGridSettingsProvider _inventoryDataGridSettingsProvider;

        public ICommand ToggleColumnCommand { get; }

        public InventoryTabItem(
            //InventoryDataGridViewModel inventoryDataGridViewModel,
            Category category,
            Predicate<object> filter,
            InventorySettingsProvider inventorySettingsProvider,
            CreateViewModel<ColumnSettingViewModel> columnSettingViewModelFactory,
            InventoryDataGridSettingsProvider inventoryDataGridSettingsProvider,
            ICategoryStore categoryStore,
            Func<ProductDataViewModel, bool> searchFilter)
        {
            _categoryStore = categoryStore;
            _category = category;

            _searchFilter = searchFilter;
            _filter = filter;

            _columnSettingViewModelFactory = columnSettingViewModelFactory;
            _inventorySettingsProvider = inventorySettingsProvider;
            _inventoryDataGridSettingsProvider = inventoryDataGridSettingsProvider;

            ColumnsView = new ObservableCollection<ColumnSettingViewModel>();
            Columns = new ObservableDictionary<string, ColumnSettingViewModel>();

        }

        public void SetViewModel(InventoryDataGridViewModel inventoryDataGridViewModel)
        {
            _view = inventoryDataGridViewModel.Inventory;
            _view.Filter = _filter;

            ContentViewModel = inventoryDataGridViewModel;
            ContentViewModel.ColumnsSettings = Columns;
        }

        public void LoadColumns(IEnumerable<ColumnInfo> columns)
        {
            Columns.Clear();
            ColumnsView.Clear();

            if (!_inventoryDataGridSettingsProvider.Categories.TryGetValue(Id, out var settings))
            {
                settings = new Dictionary<string, ColumnSetting>();
                _inventoryDataGridSettingsProvider.Categories[Id] = settings;
                _inventoryDataGridSettingsProvider.Save();
            }

            if (_inventorySettingsProvider.ColumnSchemaMap.TryGetValue(Id, out var columnSchemas)
                && ContentViewModel.ColumnExtras is null)
            {
                ContentViewModel.ColumnExtras = new ObservableDictionary<string, InventorySettings.ColumnSchema>(columnSchemas.ToDictionary(k => k.Name, v => v));
            }

            bool needsSave = false;

            foreach (var column in columns)
            {
                if (string.IsNullOrEmpty(column.Header))
                    continue;

                InventorySettings.ColumnSchema extra = null;
                ContentViewModel.ColumnExtras?.TryGetValue(column.Header, out extra);

                if (settings.TryGetValue(column.Header, out var setting))
                {
                    var coluumnSettingViewModel = _columnSettingViewModelFactory(setting);
                    coluumnSettingViewModel.Name = column.Header;
                    coluumnSettingViewModel.Hidden = !column.IgnoreVisibilityToggle;
                    coluumnSettingViewModel.IsVisible = setting.IsVisible;
                    coluumnSettingViewModel.IsDraggable = !column.IgnoreReorder;
                    coluumnSettingViewModel.DisplayIndex = setting.DisplayIndex == -1 ? column.DisplayIndex : setting.DisplayIndex;
                    Columns.Add(extra?.Field ?? column.Header, coluumnSettingViewModel);
                }
                else
                {
                    //if (column.IgnoreVisibilityToggle && column.IgnoreReorder) continue;

                    setting = new ColumnSetting() { DisplayIndex = column.DisplayIndex };
                    if (!settings.ContainsKey(column.Header))
                    {
                        settings[column.Header] = setting;
                        needsSave = true;
                    }

                    var coluumnSettingViewModel = _columnSettingViewModelFactory(setting);

                    coluumnSettingViewModel.Initialize(
                        isVisible: setting.IsVisible,
                        displayIndex: setting.DisplayIndex == -1 ? column.DisplayIndex : setting.DisplayIndex,
                        name: column.Header,
                        hidden: !column.IgnoreVisibilityToggle,
                        isDraggable: !column.IgnoreReorder
                    );

                    Columns.Add(extra?.Field ?? column.Header, coluumnSettingViewModel);
                }
            }

            if (needsSave) _inventoryDataGridSettingsProvider.Save();

            foreach (var col in Columns.Values.OrderBy(c => c.DisplayIndex))
            {
                ColumnsView.Add(col);
            }


        }

        public override void Dispose()
        {
            ContentViewModel?.Dispose();

            Columns.Clear();
            ColumnsView.Clear();
        }

        private void DeferInventoryInitialization()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _originalFilter = ContentViewModel.Inventory.Filter;

                ContentViewModel.Inventory.Filter = item =>
                {
                    if (_originalFilter != null && !_originalFilter(item))
                        return false;

                    return _searchFilter(item as ProductDataViewModel);
                };
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        public void Refresh() => ContentViewModel.Inventory?.Refresh();

        /// <summary>
        /// Gets the relative row index from this tab.
        /// </summary>
        /// <param name="product">product id</param>
        /// <returns>relative index</returns>
        public int ProductIndexOf(int product)
        {
            var relativeInventoryMap = _view.Cast<ProductDataViewModel>()
                .Select((p, index) => new { p.Item.Id, Index = index })
                .ToDictionary(i => i.Id, i => i.Index);

            if (relativeInventoryMap.TryGetValue(product, out var value))
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
    }
}

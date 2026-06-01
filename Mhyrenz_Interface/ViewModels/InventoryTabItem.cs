using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Controls.Behaviors;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.Views;
using ObservableCollections;
using static Mhyrenz_Interface.Core.TrackPropertyHelper;

namespace Mhyrenz_Interface.ViewModels
{
    public class InventoryTabItem : BaseViewModel
    {

        public string Name => _category.Name;
        public Brush Color => _categoryStore.Colors[_category.Id];
        public int Id => _category.Id;

        public ObservableDictionary<string, ColumnSettingViewModel> Columns { get; set; }

        private readonly CreateCommand<UpdateProductCommand> _updateProductCommand;

        public ObservableCollection<ColumnSettingViewModel> ColumnsView { get; set; }

        private readonly Predicate<object> _originalFilter;

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

        private readonly ICollectionView _view;
        private readonly IProductService _productService;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly ICategoryStore _categoryStore;
        private readonly Category _category;
        private readonly Func<KeyValuePair<int, ProductDataViewModel>, ProductDataViewModel, bool> _searchFilter;
        private readonly Predicate<ProductDataViewModel> _filter;
        private readonly CreateViewModel<ColumnSettingViewModel> _columnSettingViewModelFactory;
        private readonly InventorySettingsProvider _inventorySettingsProvider;
        private readonly InventoryDataGridSettingsProvider _inventoryDataGridSettingsProvider;
        private readonly CreateCommand<DirectPurchaseCommand> _directPurchaseCommand;
        private readonly IInventoryStore _inventoryStore;

        public ICommand ToggleColumnCommand { get; }

        public InventoryTabItem(
            //InventoryDataGridViewModel inventoryDataGridViewModel,
            Category category,
            Predicate<ProductDataViewModel> filter,
            InventorySettingsProvider inventorySettingsProvider,
            CreateViewModel<ColumnSettingViewModel> columnSettingViewModelFactory,
            InventoryDataGridSettingsProvider inventoryDataGridSettingsProvider,
            ICategoryStore categoryStore,
            CreateCommand<UpdateProductCommand> updateProductCommandFactory,
            CreateCommand<DirectPurchaseCommand> directPurchaseCommandFactory,
            IInventoryStore inventoryStore,
            IUndoRedoManager undoRedoManager,
            IProductService productService
            //Func<KeyValuePair<int, ProductDataViewModel>, ProductDataViewModel, bool> searchFilter
            )
        {
            _productService = productService;
            _undoRedoManager = undoRedoManager;
            _categoryStore = categoryStore;
            _category = category;

            //_searchFilter = searchFilter;
            _filter = filter;

            _columnSettingViewModelFactory = columnSettingViewModelFactory;
            _inventorySettingsProvider = inventorySettingsProvider;
            _inventoryDataGridSettingsProvider = inventoryDataGridSettingsProvider;

            ColumnsView = new ObservableCollection<ColumnSettingViewModel>();
            Columns = new ObservableDictionary<string, ColumnSettingViewModel>();

            _updateProductCommand = updateProductCommandFactory;
            _directPurchaseCommand = directPurchaseCommandFactory;
            _inventoryStore = inventoryStore;
        }

        public void SetViewModel(InventoryDataGridViewModel inventoryDataGridViewModel)
        {
            var inventoryView = inventoryDataGridViewModel.InventoryView;
            inventoryView.AttachFilter(InventoryFilter);

            foreach (var (Value, View) in inventoryView.Filtered)
            {
                View.TrackedPropertyChanged += Product_PropertyChanged;
            }

            inventoryView.ViewChanged += InventoryView_ViewChanged;

            ContentViewModel = inventoryDataGridViewModel;
            ContentViewModel.ColumnsSettings = Columns;
        }

        private void InventoryView_ViewChanged(in SynchronizedViewChangedEventArgs<ProductDataViewModel, ProductDataViewModel> e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                e.NewItem.View.TrackedPropertyChanged += Product_PropertyChanged;
            else if (e.Action == NotifyCollectionChangedAction.Remove)
                e.OldItem.View.TrackedPropertyChanged -= Product_PropertyChanged;
        }

        private bool InventoryFilter(ProductDataViewModel vm)
        {
            return _filter(vm);
        }

        private void Product_PropertyChanged(object sender, TrackedPropertyChangedEventArgs args)
        {
            if (args.Origin == PropertyChangeOrigin.UndoRedo)
                return;

            var viewModel = sender as ProductDataViewModel;

            PropertyChangeCommand<ProductVMRowInfo>.ChangedArgs changedArgs(object newVal)
            {
                return new PropertyChangeCommand<ProductVMRowInfo>.ChangedArgs
                {
                    OldValue = args.OldValue,
                    NewValue = newVal,
                    RowInfo = new ProductVMRowInfo
                    {
                        Category = viewModel.Item.CategoryId,
                        Products = new[] { viewModel.Item.Id }
                    }
                };
            }

            void commonPropHandler(Setter setter, Getter getter, int key)
            {

                void handlePropChange()
                {
                    // TODO event changed hook
                }

                _undoRedoManager.Execute(new ProductVMCommandCommonProp(
                    viewModel,
                    args: changedArgs(getter()),
                    setter: setter,
                    command: _updateProductCommand(),
                    propertyChangeHandler: handlePropChange,
                    currentViewIn: typeof(InventoryView)
                ));
            }

            void purchasePropHandler(Setter setter, Getter getter, int key)
            {

                void handlePropChange()
                {
                    App.Current.BeginInvoke(new Action(async () =>
                    {
                        if (!_inventoryStore.Store.TryGetValue(key, out var vm))
                            return;
                        vm.Item = await _productService.Get(key);
                    }));
                }

                _undoRedoManager.Execute(new ProductVMCommandPurchase(
                    key,
                    args: changedArgs(getter()),
                    setter: setter,
                    command: _directPurchaseCommand(),
                    propertyChangeHandler: handlePropChange,
                    currentViewIn: typeof(InventoryView)
                ));
            }

            TrackPropertyHelper.Build(_inventoryStore, viewModel.Item.Id, args.PropertyName)
                .Track(nameof(ProductDataViewModel.Qty), commonPropHandler)
                .Track(nameof(ProductDataViewModel.Name), commonPropHandler)
                .Track(nameof(ProductDataViewModel.RetailPrice), commonPropHandler)
                .Track(nameof(ProductDataViewModel.Barcode), commonPropHandler)
                .Track(nameof(ProductDataViewModel.Expiry), commonPropHandler)
                .Track(nameof(ProductDataViewModel.Batch), commonPropHandler)
                .Track(nameof(ProductDataViewModel.PurchaseDefaultEdit), (setter, getter, propertyName) =>
                {
                    args.OldValue = 0;
                    purchasePropHandler(setter, getter, propertyName);
                })
                .Track(nameof(ProductDataViewModel.PurchaseNormalEdit), purchasePropHandler);
        }

        public void LoadColumns(IEnumerable<ColumnInfo> columns)
        {
            // --- Phase 1: resolve settings, build all entries without touching Columns yet ---

            if (!_inventoryDataGridSettingsProvider.Categories.TryGetValue(Id, out var settings))
            {
                settings = new Dictionary<string, ColumnSetting>();
                _inventoryDataGridSettingsProvider.Categories[Id] = settings;
                _inventoryDataGridSettingsProvider.Save();
            }

            if (_inventorySettingsProvider.ColumnSchemaMap.TryGetValue(Id, out var columnSchemas)
                && ContentViewModel.ColumnExtras is null)
            {
                ContentViewModel.ColumnExtras = new ObservableDictionary<string, InventorySettings.ColumnSchema>(
                    columnSchemas.ToDictionary(k => k.Name, v => v));
            }

            bool needsSave = false;
            var newEntries = new Dictionary<string, ColumnSettingViewModel>(); // no notifications yet
            var newColumnsView = new List<ColumnSettingViewModel>();

            foreach (var column in columns)
            {
                if (string.IsNullOrEmpty(column.Header))
                    continue;

                InventorySettings.ColumnSchema extra = null;
                ContentViewModel.ColumnExtras?.TryGetValue(column.Header, out extra);

                if (!settings.TryGetValue(column.Header, out var setting))
                {
                    setting = new ColumnSetting()
                    {
                        DisplayIndex = column.DisplayIndex
                    };
                    settings[column.Header] = setting;
                    needsSave = true;
                }

                var columnSettingViewModel = _columnSettingViewModelFactory(setting);

                columnSettingViewModel.Initialize(
                    isVisible: setting.IsVisible,
                    displayIndex: setting.DisplayIndex == -1
                        ? column.DisplayIndex
                        : setting.DisplayIndex,
                    name: column.Header,
                    hidden: !column.IgnoreVisibilityToggle,
                    isDraggable: !column.IgnoreReorder
                );

                newEntries.Add(extra?.Field ?? column.Header, columnSettingViewModel);
            }

            if (needsSave)
                _inventoryDataGridSettingsProvider.Save();

            // --- Phase 2: apply atomically so bindings only cascade once the dict is complete ---

            Columns.Clear();
            ColumnsView.Clear();

            foreach (var kvp in newEntries)
                Columns.Add(kvp.Key, kvp.Value);

            foreach (var col in newEntries.Values.OrderBy(c => c.DisplayIndex))
                ColumnsView.Add(col);
        }

        public override void Dispose()
        {
            ContentViewModel?.Dispose();

            Columns.Clear();
            ColumnsView.Clear();
        }
    }
}

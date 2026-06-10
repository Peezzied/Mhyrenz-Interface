using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.Settings;
using Mhyrenz_Interface.Features.Inventory.Behaviors;
using Mhyrenz_Interface.Features.Inventory.Commands;
using Mhyrenz_Interface.Features.Inventory.Views;
using Mhyrenz_Interface.Store;
using Microsoft.Extensions.Options;
using ObservableCollections;
using static Mhyrenz_Interface.Core.PropertyTracking.TrackPropertyHelper;

namespace Mhyrenz_Interface.Features.Inventory.ViewModels
{
    public class InventoryTabItem : BaseViewModel
    {

        public string Name => _category.Name;
        public Brush Color => _categoryStore.Colors[_category.Id];
        public int Id => _category.Id;

        public ObservableDictionary<string, ColumnSettingViewModel> Columns { get; set; }

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
        private readonly ConfigManager<InventoryDataGridSettings> _inventoryDataGridSettings;
        private readonly IOptionsMonitor<InventoryDataGridSettings> _inventoryDataGridSettingsProvider;
        private readonly CreateCommand<ProductVMCommandPurchase> _productCommandPurchase;
        private readonly CreateCommand<ProductVMCommandCommonProp> _productCommandCommonProp;
        private readonly CreateCommand<ProductVMCommandPurchase> productCommandPurchase;
        private readonly CreateCommand<ProductVMCommandCommonProp> productCommandCommonProp;
        private readonly IInventoryStore _inventoryStore;

        public ICommand ToggleColumnCommand { get; }

        public InventoryTabItem(
            //InventoryDataGridViewModel inventoryDataGridViewModel,
            Category category,
            Predicate<ProductDataViewModel> filter,
            CreateViewModel<ColumnSettingViewModel> columnSettingViewModelFactory,
            ConfigManager<InventoryDataGridSettings> inventoryDataGridSettings,
            IOptionsMonitor<InventoryDataGridSettings> inventoryDataGridSettingsProvider,
            ICategoryStore categoryStore,
            CreateCommand<ProductVMCommandPurchase> productCommandPurchase,
            CreateCommand<ProductVMCommandCommonProp> productCommandCommonProp,
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
            _inventoryDataGridSettings = inventoryDataGridSettings;
            _inventoryDataGridSettingsProvider = inventoryDataGridSettingsProvider;

            ColumnsView = new ObservableCollection<ColumnSettingViewModel>();
            Columns = new ObservableDictionary<string, ColumnSettingViewModel>();
            _productCommandPurchase = productCommandPurchase;
            _productCommandCommonProp = productCommandCommonProp;
            _inventoryStore = inventoryStore;
        }

        public void SetViewModel(InventoryDataGridViewModel inventoryDataGridViewModel)
        {
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

                _undoRedoManager.Execute(_productCommandCommonProp(new ProductVMCommandCommonProp.DTO
                {
                    Product = viewModel.Item,
                    ChangedArgs = changedArgs(getter()),
                    Setter = setter,
                    PropertyChangeHandler = handlePropChange,
                    CurrentViewIn = typeof(InventoryView)
                }));
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

                _undoRedoManager.Execute(_productCommandPurchase(new ProductVMCommandPurchase.DTO
                {
                    ProductId = key,
                    Setter = setter,
                    ChangedArgs = changedArgs(getter()),
                    PropertyChangeHandler = handlePropChange,
                    CurrentViewIn = typeof(InventoryView)
                }));
            }

            TrackPropertyHelper.Build(_inventoryStore, viewModel.Item.Id, args.PropertyName)
                .Track(nameof(ProductDataViewModel.Qty), commonPropHandler)
                .Track(nameof(ProductDataViewModel.Name), commonPropHandler)
                .Track(nameof(ProductDataViewModel.RetailPrice), commonPropHandler)
                .Track(nameof(ProductDataViewModel.Barcode), commonPropHandler)
                .Track(nameof(ProductDataViewModel.Expiry), commonPropHandler)
                .Track(nameof(ProductDataViewModel.Batch), commonPropHandler)
                .Track(nameof(ProductDataViewModel.PurchaseDefaultEdit), (setter, getter, key) =>
                {
                    args.OldValue = 0;
                    purchasePropHandler(setter, getter, key);
                })
                .Track(nameof(ProductDataViewModel.PurchaseNormalEdit), purchasePropHandler);
        }

        public void LoadColumns(IEnumerable<ColumnInfo> columns)
        {

            bool needsSave = false;
            var newEntries = new Dictionary<string, ColumnSettingViewModel>(); // no notifications yet
            var newColumnsView = new List<ColumnSettingViewModel>();

            var settings = (_inventoryDataGridSettingsProvider.CurrentValue ?? new InventoryDataGridSettings())
                .ToDictionary(k => k.Header, v => v);


            foreach (var column in columns)
            {
                if (string.IsNullOrEmpty(column.Header))
                    continue;

                if (!settings.TryGetValue(column.Header, out var setting))
                {
                    setting = new InventoryDataGridColumnSetting()
                    {
                        Header = column.Header,
                        DisplayIndex = column.DisplayIndex
                    };
                    settings[column.Header] = setting;
                    needsSave = true;
                }

                var columnSettingViewModel = _columnSettingViewModelFactory(new InventoryDataGridColumnSetting(setting));

                columnSettingViewModel.Initialize(
                    isVisible: setting.IsVisible,
                    displayIndex: setting.DisplayIndex,
                    name: setting.Header,
                    hidden: !column.IgnoreVisibilityToggle,
                    isDraggable: !column.IgnoreReorder,
                    placeOrderBound: column.PlaceOrderBound
                );

                newEntries.Add(column.Header, columnSettingViewModel);
            }

            if (needsSave)
                _inventoryDataGridSettings.Save(new InventoryDataGridSettings(settings.Values));

            // --- Phase 2: apply atomically so bindings only cascade once the dict is complete ---

            Columns.Clear();
            ColumnsView.Clear();

            foreach (var kvp in newEntries)
                Columns.Add(kvp.Key, kvp.Value);

            foreach (var col in newEntries.Values.OrderBy(c => c.DisplayIndex))
                ColumnsView.Add(col);

            OnColumnsLoaded();
        }

        private void OnColumnsLoaded()
        {
            ColumnsLoaded?.Invoke();
        }

        public override void Dispose()
        {
            if (_disposed)
                return;

            Unload();

            ColumnsLoaded = null;
            ColumnsChanged = null;

            ContentViewModel?.Dispose();
            ContentViewModel = null;

            Columns?.Clear();
            ColumnsView?.Clear();

            base.Dispose();

            _disposed = true;
        }

        public event Action ColumnsChanged;

        public event Action ColumnsLoaded;

        internal void OnColumnsChanged()
        {
            ColumnsChanged?.Invoke(); // update the view

            _inventoryDataGridSettings.Save(new InventoryDataGridSettings(ColumnsView.Select(x => x.ColumnSetting)));
        }

        private bool _reorderEnabled = true;
        public bool ReorderEnabled
        {
            get => _reorderEnabled;
            set
            {
                _reorderEnabled = value;
                OnPropertyChanged(nameof(ReorderEnabled));
            }
        }

        internal void PlaceOrderMode(bool placeOrderIsOpen)
        {
            ReorderEnabled = !placeOrderIsOpen;
            if (placeOrderIsOpen)
            {
                foreach (var item in ColumnsView)
                {
                    if (item.PlaceOrderBound)
                    {
                        item.IsVisible = true;
                    }
                    else
                    {
                        item.IsVisible = false;
                    }
                }
                _inventoryDataGridSettings.Save(new InventoryDataGridSettings(ColumnsView.Select(x => x.ColumnSetting)));
            }
            else
            { // restore
                foreach (var item in _inventoryDataGridSettingsProvider.CurrentValue)
                {
                    var col = Columns[item.Header];
                    col.IsVisible = item.IsVisible;
                }
                _inventoryDataGridSettings.Save(new InventoryDataGridSettings(ColumnsView.Select(x => x.ColumnSetting)));
            }

        }

        public void Unload()
        {
            if (!_isLoaded)
                return;

            if (ContentViewModel != null)
            {
                var inventoryView = ContentViewModel.InventoryView;

                inventoryView.ViewChanged -= InventoryView_ViewChanged;

                foreach (var (_, view) in inventoryView.Filtered)
                    view.TrackedPropertyChanged -= Product_PropertyChanged;
            }

            _isLoaded = false;
        }

        private bool _isLoaded;
        private bool _disposed;

        public void Load()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(InventoryTabItem));

            if (_isLoaded)
                return;

            if (ContentViewModel == null)
            {
                // ContentViewModel must be assigned by InventoryViewModel before Load,
                // unless you move the factory into this class.
                return;
            }

            var inventoryView = ContentViewModel.InventoryView;

            inventoryView.AttachFilter(InventoryFilter);

            foreach (var (_, view) in inventoryView.Filtered)
                view.TrackedPropertyChanged += Product_PropertyChanged;

            inventoryView.ViewChanged += InventoryView_ViewChanged;

            ContentViewModel.ColumnsSettings = Columns;

            _isLoaded = true;
        }
    }
}

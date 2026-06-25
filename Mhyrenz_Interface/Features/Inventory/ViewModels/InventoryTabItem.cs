using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Models.Settings;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.Settings;
using Mhyrenz_Interface.Features.Inventory.Behaviors;
using Mhyrenz_Interface.Features.Inventory.Commands;
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
        public bool IsPharma => _category.IsPharma;

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
        private readonly ICategoryStore _categoryStore;
        private readonly Category _category;
        private readonly Func<KeyValuePair<int, ProductDataViewModel>, ProductDataViewModel, bool> _searchFilter;
        private readonly Predicate<ProductDataViewModel> _filter;
        private readonly CreateViewModel<ColumnSettingViewModel> _columnSettingViewModelFactory;
        private readonly ConfigManager<InventoryDataGridSettings> _inventoryDataGridSettings;
        private readonly IOptionsMonitor<InventoryDataGridSettings> _inventoryDataGridSettingsProvider;
        private readonly CreateCommand<ProductVMCommandPurchase> _productCommandPurchase;
        private readonly CreateCommand<ProductVMCommandCommonProp> _productCommandCommonProp;
        private readonly CreateCommand<ProductVMCommandMarkupRate> _productCommandMarkupRate;
        private readonly CreateCommand<ProductVMCommandPurchase> productCommandPurchase;
        private readonly CreateCommand<ProductVMCommandCommonProp> productCommandCommonProp;
        private readonly IInventoryStore _inventoryStore;

        public ICommand ToggleColumnCommand { get; }

        public InventoryTabItem(
            Category category,
            Predicate<ProductDataViewModel> filter,
            CreateViewModel<ColumnSettingViewModel> columnSettingViewModelFactory,
            ICategoryStore categoryStore,
            CreateCommand<ProductVMCommandPurchase> productCommandPurchase,
            CreateCommand<ProductVMCommandCommonProp> productCommandCommonProp,
            CreateCommand<ProductVMCommandMarkupRate> productCommandMarkupRate,
            IInventoryStore inventoryStore,
            IProductService productService
            )
        {
            _productService = productService;
            _categoryStore = categoryStore;
            _category = category;

            //_searchFilter = searchFilter;
            _filter = filter;

            _columnSettingViewModelFactory = columnSettingViewModelFactory;

            ColumnsView = new ObservableCollection<ColumnSettingViewModel>();
            Columns = new ObservableDictionary<string, ColumnSettingViewModel>();
            _productCommandPurchase = productCommandPurchase;
            _productCommandCommonProp = productCommandCommonProp;
            _productCommandMarkupRate = productCommandMarkupRate;
            _inventoryStore = inventoryStore;
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

            var changedArgs = new PropertyChangeCommand<ProductVMRowInfo>.ChangedArgs
            {
                OldValue = args.OldValue,
                NewValue = args.NewValue,
                RowInfo = new ProductVMRowInfo
                {
                    Category = viewModel.Item.CategoryId,
                    Products = new[] { viewModel.Item.Id }
                }
            };

            Action<Product> updater = null;

            void commonPropHandler(Setter setter, int key)
            {
                App.UndoRedoManager.Execute(_productCommandCommonProp(new ProductVMCommandCommonProp.DTO
                {
                    Updater = updater,
                    PropertyName = args.PropertyName,
                    Product = viewModel.Item,
                    ChangedArgs = changedArgs,
                    Setter = setter
                }));
                updater = null;
            }

            void purchasePropHandler(Setter setter, int key)
            {
                App.UndoRedoManager.Execute(_productCommandPurchase(new ProductVMCommandPurchase.DTO
                {
                    ProductId = key,
                    Setter = setter,
                    ChangedArgs = changedArgs
                }));
            }

            TrackPropertyHelper.Build(_inventoryStore, viewModel.Item.Id, args.PropertyName)
                .Track(nameof(ProductDataViewModel.Qty), commonPropHandler)
                .Track(nameof(ProductDataViewModel.Name), commonPropHandler)
                .Track(nameof(ProductDataViewModel.RetailPrice), commonPropHandler)
                .Track(nameof(ProductDataViewModel.Expiry), commonPropHandler)
                .Track(nameof(ProductDataViewModel.Batch), commonPropHandler)
                .Track(nameof(ProductDataViewModel.Barcode), async (s, k) =>
                {
                    var barcode = args.NewValue as string;
                    if (await _productService.IsBarcodeUnique(barcode) || barcode.IsNullOrEmpty())
                    {
                        viewModel.SetValue(nameof(viewModel.Barcode), barcode);
                        commonPropHandler(s, k);
                    }
                    else
                    {
                        Growl.Warning(new GrowlInfo
                        {
                            Message = $"The barcode \"{barcode}\" is already taken.",
                            ShowDateTime = false,
                        });
                    }
                }, setterBarcode)
                .Track(nameof(ProductDataViewModel.CostPrice), (s, k) =>
                {
                    updater = p =>
                    {
                        p.CostPrice = viewModel.CostPrice;
                        p.RetailPrice = viewModel.RetailPrice;
                    };
                    commonPropHandler(s, k);
                })
                .Track(nameof(ProductDataViewModel.MarkupRate), (s, k) =>
                {
                    updater = p =>
                    {
                        p.MarkupRate = viewModel.MarkupRate;
                        p.RetailPrice = viewModel.RetailPrice;
                    };
                    commonPropHandler(s, k);
                })
                .Track(nameof(ProductDataViewModel.PurchaseNormalEdit), purchasePropHandler)
                .Track(nameof(ProductDataViewModel.PurchaseDefaultEdit), (setter, key) =>
                {
                    args.OldValue = 0;
                    purchasePropHandler(setter, key);
                })
                .Track(nameof(PharmaDetailsViewModel.GenericName), (setter, key) =>
                {
                    // TODO Generic name tracker
                }, setterPharmaDetals);

            void setterBarcode(object val, PropertyChangeOrigin origin)
            {
                if (!_inventoryStore.Store.TryGetValue(viewModel.Item.Id, out var vm))
                    return;

                vm.TrackingOrigin = origin;

                try
                {
                    vm.SetValue(nameof(vm.Barcode), val);
                }
                finally
                {
                    vm.TrackingOrigin = default;
                }
            }

            void setterPharmaDetals(object val, PropertyChangeOrigin origin)
            {
                if (!_inventoryStore.Store.TryGetValue(viewModel.Item.Id, out var vm))
                    return;

                var property = vm.GetType().GetProperty(nameof(ProductDataViewModel.PharmaDetails))
                    .GetValue(vm);
                vm.TrackingOrigin = origin;

                try
                {
                    property.GetType().GetProperty(args.PropertyName).SetValue(vm, val);
                }
                finally
                {
                    vm.TrackingOrigin = default;
                }
            }
        }

        public void LoadColumns(IEnumerable<ColumnInfo> columns)
        {

            bool needsSave = false;
            var newEntries = new Dictionary<string, ColumnSettingViewModel>(); // no notifications yet
            var newColumnsView = new List<ColumnSettingViewModel>();

            var settings = (InventoryDataGridSettingsStore.Load() ?? new InventoryDataGridSettings())
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
                        DisplayIndex = column.DisplayIndex,
                        PharmaColumn = column.PharmaColumn
                    };
                    settings[column.Header] = setting;
                    needsSave = true;
                }

                // do not create columnsettingvm when the column is a pharma
                if (column.PharmaColumn && !IsPharma)
                    continue;

                var columnSettingViewModel = _columnSettingViewModelFactory(setting);

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
                InventoryDataGridSettingsStore.Save(new InventoryDataGridSettings(settings.Values));

            // --- Phase 2: apply atomically so bindings only cascade once the dict is complete ---

            Columns.Clear();
            ColumnsView.Clear();

            foreach (var kvp in newEntries)
                Columns.Add(kvp.Key, kvp.Value);

            foreach (var col in newEntries.Values.OrderBy(c => c.DisplayIndex))
            {
                ColumnsView.Add(col);
                col.PropertyChanged += Col_PropertyChanged;
            }

            OnColumnsLoaded();
        }

        private bool _placeOrderMode = false;

        private void Col_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ColumnSettingViewModel.IsVisible) && !_placeOrderMode)
            {
                InventoryDataGridSettingsStore.Save(new InventoryDataGridSettings(ColumnsView.Select(x => x.ColumnSetting)));
            }
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
            _placeOrderMode = true;
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
            }
            else
            { // restore
                foreach (var item in InventoryDataGridSettingsStore.Load())
                {
                    if (Columns.TryGetValue(item.Header, out var col))
                        col.IsVisible = item.IsVisible;
                }
            }
            _placeOrderMode = false;
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Controls;
using Mhyrenz_Interface.Controls.Behaviors;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.EntityFrameworkCore.Internal;

namespace Mhyrenz_Interface.ViewModels
{
    public class InventoryDataGridViewModel : BaseViewModel
    {
        public InventoryDataGridLayout Layout { get; }

        private readonly ICollectionView _inventory;
        public ICollectionView Inventory => _inventory;

        public event Action<ActionType> UndoRedoEvent;

        public event Action CommitEdits;
        public ICommand DeleteCommand { get; set; }

        private IEnumerable<ProductDataViewModel> _selectedItems;
        /// <summary>
        /// Selected rows.
        /// </summary>
        public IEnumerable<ProductDataViewModel> SelectedItems
        {
            get => _selectedItems;
            set
            {
                _selectedItems = value;

                SelectedItemsChanged?.Invoke(value.Any());
            }
        }

        public IEnumerable<DataGridColumn> DataGridColumnOwner { get; set; }

        private object _selectedItem;
        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public ObservableDictionary<string, ColumnSettingViewModel> ColumnsSettings { get; set; }

        private bool _idColumn;
        public bool IdColumn
        {
            get => _idColumn;
            set
            {
                _idColumn = value;
                OnPropertyChanged(nameof(IdColumn));
            }
        }

        private bool _genericNameColumn;
        public bool GenericNameColumn
        {
            get => _genericNameColumn;
            set
            {
                _genericNameColumn = value;
                OnPropertyChanged(nameof(GenericNameColumn));
            }
        }

        private bool _batchColumn;
        public bool BatchColumn
        {
            get => _batchColumn;
            set
            {
                _batchColumn = value;
                OnPropertyChanged(nameof(BatchColumn));
            }
        }

        private bool _expiryColumn;
        public bool ExpiryDateColumn
        {
            get => _expiryColumn;
            set
            {
                _expiryColumn = value;
                OnPropertyChanged(nameof(ExpiryDateColumn));
            }
        }

        private bool _supplierColumn;
        public bool SupplierColumn
        {
            get => _supplierColumn;
            set
            {
                _supplierColumn = value;
                OnPropertyChanged(nameof(SupplierColumn));
            }
        }

        private bool _barcodeColumn;
        public bool BarcodeColumn
        {
            get
            {
                return _barcodeColumn;
            }
            set
            {
                _barcodeColumn = value;
                OnPropertyChanged(nameof(BarcodeColumn));
            }
        }


        public ObservableDictionary<string, InventorySettings.ColumnSchema> ColumnExtras { get; set; }

        public SelectionRowsInfo SelectionInfo { get; set; }

        public Func<DataGridCell> GetCell { get; set; }

        public event Action<ProductDataViewModel> Purchased;
        public event Action<bool> SelectedItemsChanged;
        public event Action SwitchSelectedItem;
        public event Action OnLoad;

        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;

        public ICommand ToggleColumnCommand { get; }
        public bool IsEditCancelled { get; private set; }

        private readonly IUndoRedoManager _undoRedoManager;

        public class SelectionRowsInfo
        {
            public SelectionRowsInfo(int index, int[] items, bool isDifferent, bool canSelect = true)
            {
                Index = index;
                Items = items;
                IsDifferent = isDifferent;
                CanSelect = canSelect;
            }

            public int Index { get; set; }
            public int[] Items { get; set; }
            public bool IsDifferent { get; set; }
            public bool CanSelect { get; set; }
        }

        public InventoryDataGridViewModel(IUndoRedoManager undoRedoManager, IProductService productService, IInventoryStore inventoryStore, NavigationViewModel viewHost, InventoryDataGridLayout layout)
        {
            _undoRedoManager = undoRedoManager;
            _inventoryStore = inventoryStore;
            _productService = productService;

            _inventory = new ListCollectionView(_inventoryStore.Products);
            ApplyDefaultSort();

            Layout = layout;

            DeleteCommand = new DeleteCommand(_productService, _inventoryStore, _undoRedoManager);
        }

        private void ApplyDefaultSort()
        {
            if (_inventory is ListCollectionView view)
            {
                view.CustomSort = Comparer<ProductDataViewModel>.Create((a, b) =>
                    StringComparer.CurrentCultureIgnoreCase.Compare(a?.Name, b?.Name)
                );

                view.Refresh();
            }
        }

        public void Load()
        {
            // FIXME: the subscription may cause the lag
            _undoRedoManager.UndoRedoEvent += UndoRedoManager_UndoRedoEvent;
            _inventoryStore.PurchaseEvent += InventoryStore_PurchaseEvent;
            IsEditCancelled = false;
            OnLoad?.Invoke();
        }

        public override void Dispose()
        {
            IsEditCancelled = true;
            CommitEdits?.Invoke();
            _undoRedoManager.UndoRedoEvent -= UndoRedoManager_UndoRedoEvent;
            _inventoryStore.PurchaseEvent -= InventoryStore_PurchaseEvent;  
        }

        public void SelectItem(bool isDiff, int index, int[] selection)
        {
            SelectionInfo = new SelectionRowsInfo(index, selection, isDiff);

            SwitchSelectedItem?.Invoke();
        }

        #region "Event handlers"
        private void UndoRedoManager_UndoRedoEvent(ActionType obj, UndoRedoEventArgs e)
        {
            if (e.CurrentView is InventoryViewModel inventoryGridHost)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    inventoryGridHost.RowIntoView(_inventoryStore.LastProductChanged.Category, _inventoryStore.LastProductChanged.ChangedProductInfo.Value.Products);
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        private void InventoryStore_PurchaseEvent(object sender, InventoryStoreEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Purchased?.Invoke(e.Product);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
        #endregion
    }
}

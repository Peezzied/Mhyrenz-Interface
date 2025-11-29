using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Mhyrenz_Interface.ViewModels
{
    public enum Columns
    {
        IdColumn,
        GenericNameColumn,
        BatchColumn,
        ExpiryColumn,
        SupplierColumn
    }

    public class InventoryDataGridViewModel : BaseViewModel
    {
        private ICollectionView _inventory;
        public ICollectionView Inventory
        {
            get => _inventory;
            set
            {
                if (_inventory != value)
                {
                    _inventory = value;
                    _inventory.SortDescriptions.Clear();
                    _inventory.SortDescriptions.Add(new SortDescription(nameof(ProductDataViewModel.Name), ListSortDirection.Ascending));
                    OnPropertyChanged(nameof(Inventory));
                }
            }
        }
        public event Action<ActionType> UndoRedoEvent;

        public event Action CommitEdits;
        public ICommand DeleteCommand { get; set; }

        private IEnumerable<ProductDataViewModel> _selectedItems;
        public IEnumerable<ProductDataViewModel> SelectedItems
        {
            get => _selectedItems;
            set
            {
                _selectedItems = value;

                SelectedItemsChanged?.Invoke(value.Any());
            }
        }

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

        private bool _isGeneric;
        public bool IsGeneric
        {
            get => _isGeneric;
            set
            {
                _isGeneric = value;
                OnPropertyChanged(nameof(IsGeneric));
            }
        }

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
        public bool ExpiryColumn
        {
            get => _expiryColumn;
            set
            {
                _expiryColumn = value;
                OnPropertyChanged(nameof(ExpiryColumn));
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

        public ObservableDictionary<string, InventorySettings.ColumnSchema> ColumnExtras { get; set; }

        public SelectionRowsInfo SelectionInfo { get; set; }

        public Func<DataGridCell> GetCell { get; set; }

        public event Action<ProductDataViewModel> Purchased;
        public event Action<bool> SelectedItemsChanged;
        public event Action SwitchSelectedItem;

        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;

        public ICommand ToggleColumnCommand { get; }
        public bool IsEditCancelled { get; private set; }

        private readonly IUndoRedoManager _undoRedoManager;

        public class SelectionRowsInfo
        {
            public SelectionRowsInfo(int index, IEnumerable<ProductDataViewModel> items, bool isDifferent, bool canSelect = true)
            {
                Index = index;
                Items = items;
                IsDifferent = isDifferent;
                CanSelect = canSelect;
            }

            public int Index { get; set; }
            public IEnumerable<ProductDataViewModel> Items { get; set; }
            public bool IsDifferent { get; set; }
            public bool CanSelect { get; set; }
        }

        public InventoryDataGridViewModel(IUndoRedoManager undoRedoManager, IProductService productService, IInventoryStore inventoryStore, NavigationViewModel viewHost)
        {
            _undoRedoManager = undoRedoManager;
            _inventoryStore = inventoryStore;
            _productService = productService;
            _inventoryStore = inventoryStore;


            DeleteCommand = new DeleteCommand(_productService, _inventoryStore, _undoRedoManager);
        }

        public void Load()
        {
            _undoRedoManager.UndoRedoEvent += UndoRedoManager_UndoRedoEvent;
            _inventoryStore.PurchaseEvent += InventoryStore_PurchaseEvent;
            IsEditCancelled = false;
        }

        public override void Dispose()
        {
            IsEditCancelled = true;
            CommitEdits?.Invoke();
            _undoRedoManager.UndoRedoEvent -= UndoRedoManager_UndoRedoEvent;
        }

        public void SelectItem(bool isDiff, int index, IEnumerable<ProductDataViewModel> selection)
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
                    inventoryGridHost.RowIntoView(_inventoryStore.LastProductChanged.Products);
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

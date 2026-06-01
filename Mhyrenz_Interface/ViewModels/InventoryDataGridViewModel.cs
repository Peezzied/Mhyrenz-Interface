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
using ObservableCollections;

namespace Mhyrenz_Interface.ViewModels
{
    public class InventoryDataGridViewModel : BaseViewModel, IEditCancelState
    {
        public NotifyCollectionChangedSynchronizedViewList<ProductDataViewModel> Inventory { get; }

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

        private readonly IInventoryStore _inventoryStore;

        public ISynchronizedView<ProductDataViewModel, ProductDataViewModel> InventoryView { get; private set; }
        public ICommand ToggleColumnCommand { get; }
        public bool IsEditCancelled { get; private set; }

        private readonly IUndoRedoManager _undoRedoManager;

        public class SelectionRowsInfo
        {
            public SelectionRowsInfo(int[] items, bool canSelectTab = true)
            {
                Items = items;
                CanSelect = canSelectTab;
            }
            public int[] Items { get; set; }
            public bool CanSelect { get; set; }
        }

        public InventoryDataGridViewModel(IUndoRedoManager undoRedoManager, IInventoryStore inventoryStore, CreateCommand<DeleteCommand> deleteCommand, NavigationViewModel viewHost)
        {
            _undoRedoManager = undoRedoManager;
            _inventoryStore = inventoryStore;

            InventoryView = _inventoryStore.Store.Source.CreateView(v => v);
            Inventory = InventoryView.ToNotifyCollectionChanged(
                SynchronizationContextCollectionEventDispatcher.Current);

            DeleteCommand = deleteCommand();
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

        public void SelectItem(bool canSelectTab, int[] selection)
        {
            SelectionInfo = new SelectionRowsInfo(selection, canSelectTab);

            SwitchSelectedItem?.Invoke();
        }

        #region "Event handlers"
        private void UndoRedoManager_UndoRedoEvent(ActionType obj, UndoRedoEventArgs e)
        {
            // TODO apply and evaluate IsReadonly flag
            if (e.CurrentView is NavigationViewModel inventoryGridHost)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    e.Command.SideEffect?.Invoke(inventoryGridHost);
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

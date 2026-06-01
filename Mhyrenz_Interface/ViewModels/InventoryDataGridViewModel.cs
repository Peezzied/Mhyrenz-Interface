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

        public ObservableDictionary<string, ColumnSettingViewModel> ColumnsSettings { get; set; }

        public ObservableDictionary<string, InventorySettings.ColumnSchema> ColumnExtras { get; set; }

        public SelectionRowsInfo SelectionInfo { get; set; }

        public event Action<bool> SelectedItemsChanged;
        public event Action SwitchSelectedItem;
        public event Action OnLoad;

        private readonly IInventoryStore _inventoryStore;

        public ISynchronizedView<ProductDataViewModel, ProductDataViewModel> InventoryView { get; private set; }
        public bool IsEditCancelled { get; private set; }

        private readonly IUndoRedoManager _undoRedoManager;

        public InventoryDataGridViewModel(IUndoRedoManager undoRedoManager, IInventoryStore inventoryStore, CreateCommand<DeleteCommand> deleteCommand, NavigationViewModel viewHost)
        {
            _undoRedoManager = undoRedoManager;
            _inventoryStore = inventoryStore;

            InventoryView = _inventoryStore.Store.Source.CreateView(v => v);
            Inventory = InventoryView.ToNotifyCollectionChanged(
                SynchronizationContextCollectionEventDispatcher.Current);

            DeleteCommand = deleteCommand();
        }

        private bool _isReadOnly = false;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                OnPropertyChanged(nameof(IsReadOnly));
            }
        }

        public void Load()
        {
            // FIXME: the subscription may cause the lag
            _undoRedoManager.UndoRedoEvent += UndoRedoManager_UndoRedoEvent;
            IsEditCancelled = false;
            OnLoad?.Invoke();
        }

        public override void Dispose()
        {
            IsEditCancelled = true;
            CommitEdits?.Invoke();
            _undoRedoManager.UndoRedoEvent -= UndoRedoManager_UndoRedoEvent;

        }

        public void SelectItem(bool canSelectTab, int[] selection)
        {
            SelectionInfo = new SelectionRowsInfo(selection, canSelectTab);

            SwitchSelectedItem?.Invoke();
        }

        #region "Event handlers"
        private void UndoRedoManager_UndoRedoEvent(ActionType obj, UndoRedoEventArgs e)
        {
            if (e.CurrentView is NavigationViewModel inventoryGridHost && !IsReadOnly)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    e.Command.SideEffect?.Invoke(inventoryGridHost);
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }
        #endregion

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
    }
}

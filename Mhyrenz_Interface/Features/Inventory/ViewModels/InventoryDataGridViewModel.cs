using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Features.Inventory.Commands;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;
using ObservableCollections;

namespace Mhyrenz_Interface.Features.Inventory.ViewModels
{
    public class InventoryDataGridViewModel : BaseViewModel, IEditCancelState, IFlashRequestable
    {
        public event Action CommitEdits;

        private IEnumerable<ProductDataViewModel> _selectedItems;
        private CollectionViewSource _cvs;

        private ICollectionView _inventoryView;

        public ICollectionView Inventory
        {
            get
            {
                if (_synchronizedView == null)
                    return null;

                if (_inventoryView == null)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _cvs = new CollectionViewSource { Source = _synchronizedView };
                        _cvs.SortDescriptions.Add(
                            new SortDescription(nameof(ProductDataViewModel.Name), ListSortDirection.Ascending)
                        );

                        IsEditCancelled = false;
                        _inventoryView = _cvs.View;
                    });
                }

                return _inventoryView;
            }
        }

        /// <summary>
        /// Selected rows.
        /// </summary>
        public IEnumerable<ProductDataViewModel> SelectedItems
        {
            get => _selectedItems;
            set
            {
                _selectedItems = value;

                SelectedItemsChanged?.Invoke();
            }
        }

        public ObservableDictionary<string, ColumnSettingViewModel> ColumnsSettings { get; set; }

        public SelectionRowsInfo SelectionInfo { get; set; }

        public event Action SelectedItemsChanged;
        public event Action SwitchSelectedItem;
        public event EventHandler<RowFlashRequestedEventArgs> FlashRequested;

        private readonly IInventoryStore _inventoryStore;
        private readonly CreateCommand<DeleteCommand> _deleteCommand;

        public ISynchronizedView<ProductDataViewModel, ProductDataViewModel> InventoryView { get; private set; }

        private readonly NotifyCollectionChangedSynchronizedViewList<ProductDataViewModel> _synchronizedView;

        public bool IsEditCancelled { get; private set; }

        private readonly IUndoRedoManager _undoRedoManager;

        public InventoryDataGridViewModel(IUndoRedoManager undoRedoManager, ISessionStore sessionStore, IInventoryStore inventoryStore, BaseViewModel viewHost)
        {
            _undoRedoManager = undoRedoManager;
            _inventoryStore = inventoryStore;

            SessionClosed = sessionStore.CurrentSession == null;

            InventoryView = _inventoryStore.Store.Source.CreateView(v => v);

            _synchronizedView = InventoryView.ToNotifyCollectionChanged(
                SynchronizationContextCollectionEventDispatcher.Current);
        }

        public bool SessionClosed { get; set; }

        public override void Dispose()
        {
            IsEditCancelled = true;
            CommitEdits?.Invoke();
        }

        public void SelectItem(bool canSelectTab, int[] selection)
        {
            SelectionInfo = new SelectionRowsInfo(selection, canSelectTab);

            SwitchSelectedItem?.Invoke();
        }

        public async Task RequestFlash(IFlashReceiver item, DataGridFlashBehavior.OperationType type)
        {
            await FlashRequested.RequestFlash(item, type);
        }

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

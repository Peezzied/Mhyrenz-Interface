using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
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
    public class InventoryDataGridVmDTO
    {
        public IEnumerable<ProductDataViewModel> ProductData { get; set; }
        public Action DeleteHandle { get; set; }
        public Action RemoveItemsHandler { get; set; }
    }

    public class InventoryDataGridViewModel : BaseViewModel
    {
        public InventoryDataGridViewModel(IUndoRedoManager undoRedoManager, IProductService productService, IInventoryStore inventoryStore, NavigationViewModel viewHost)
        {
            _undoRedoManager = undoRedoManager;
            _inventoryStore = inventoryStore;
            _viewHost = viewHost;
            _productService = productService;
            _inventoryStore = inventoryStore;

            _undoRedoManager.UndoRedoEvent += UndoRedoManager_UndoRedoEvent;
            _inventoryStore.PurchaseEvent += InventoryStore_PurchaseEvent;

            DeleteCommand = new DeleteCommand(_productService, _inventoryStore, _undoRedoManager);
        }


        public override void Dispose()
        {
            _undoRedoManager.UndoRedoEvent -= UndoRedoManager_UndoRedoEvent;
        }

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
            
        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;
        private readonly NavigationViewModel _viewHost;
        private readonly IUndoRedoManager _undoRedoManager;

        private void UndoRedoManager_UndoRedoEvent(ActionType obj, UndoRedoEventArgs e)
        {
            if (e.CurrentView is InventoryGridHost inventoryGridHost)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ProductDataViewModel product;
                    int tabSelect = -1;
                    bool canSelect = true;

                    try
                    {
                        product = _inventoryStore.Products[_inventoryStore.LastProductChanged.Index];
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        product = _inventoryStore.Products[_inventoryStore.LastProductChanged.Index - 1];
                        tabSelect = _inventoryStore.LastProductChanged.Product.CategoryId;
                        canSelect = false;
                    }

                    inventoryGridHost.RowIntoView(product, (tabSelect, canSelect));
                    UndoRedoEvent?.Invoke(obj);
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        public event Action<ActionType> UndoRedoEvent;
        public ICommand DeleteCommand { get; set; }

        private IEnumerable<ProductDataViewModel> _selectedItems;
        public IEnumerable<ProductDataViewModel> SelectedItems
        {
            get => _selectedItems;
            set
            {
                _selectedItems = value;

                bool state;
                if (value.Any())
                    state = true;
                else state = false;

                SelectedItemsChanged?.Invoke(state);
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

        public event Action<ProductDataViewModel> Purchased;
        public event Action<bool> SelectedItemsChanged;
        public event Action<bool> SwitchSelectedItem;
        public Action RemoveItems { get; internal set; }

        private void InventoryStore_PurchaseEvent(object sender, InventoryStoreEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Purchased?.Invoke(e.Product);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        public bool IsDiff { get; set; }
        public int SwitchSelectItem { get; set; } = -1;

        public Func<DataGridCell> GetCell { get; set; }

        public void SelectItem(bool isDiff, int index, bool canSelect)
        {
            IsDiff = isDiff;
            SwitchSelectItem = index;

            SwitchSelectedItem?.Invoke(canSelect);
        }

    }
}

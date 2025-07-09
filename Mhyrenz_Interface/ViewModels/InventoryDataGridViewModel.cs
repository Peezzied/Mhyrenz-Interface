using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        public Action RemoveItems { get; set; }
    }

    public class InventoryDataGridViewModel: BaseViewModel
    {
        public InventoryDataGridViewModel()
        {
            
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
                    OnPropertyChanged(nameof(Inventory));
                }
            }
        }

        private readonly IProductService _productService;
        private readonly IInventoryStore _inventoryStore;

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
        public event Action SwitchSelectedItem;
        public Action RemoveItems { get; internal set; }

        public InventoryDataGridViewModel(IProductService productService, IInventoryStore inventoryStore)
        {
            _productService = productService;
            _inventoryStore = inventoryStore;

            _inventoryStore.PurchaseEvent += InventoryStore_PurchaseEvent;

            DeleteCommand = new DeleteCommand(_productService); // TO UNDO MANAGER
        }

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

        public void SelectItem(bool isDiff, int index)
        {
            IsDiff = isDiff;
            SwitchSelectItem = index;

            SwitchSelectedItem?.Invoke();
        }

    }
}

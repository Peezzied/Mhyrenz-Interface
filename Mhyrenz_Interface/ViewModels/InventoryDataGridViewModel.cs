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

        public event Action<bool> SelectedItemsChanged;
        public event Action SwitchSelectedItem;
        public Action RemoveItems { get; internal set; }

        public InventoryDataGridViewModel(IProductService productService)
        {
            _productService = productService;

            DeleteCommand = new DeleteCommand(_productService); // TO UNDO MANAGER
        }

        public bool IsDiff { get; set; }
        public int SwitchSelectItem { get; set; } = -1;

        public void SelectItem(bool isDiff, int index)
        {
            IsDiff = isDiff;
            SwitchSelectItem = index;

            SwitchSelectedItem?.Invoke();
        }
    }
}

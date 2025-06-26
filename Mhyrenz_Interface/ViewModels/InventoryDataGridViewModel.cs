using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Mhyrenz_Interface.ViewModels
{
    public class InventoryDataGridVmDTO
    {
        public IEnumerable<ProductDataViewModel> ProductData { get; set; }
        public Action DeleteHandle { get; internal set; }
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

        public InventoryDataGridViewModel(IProductService productService)
        {
            _productService = productService;

            DeleteCommand = new RelayCommand(DeleteCommandExecute); // TO UNDO MANAGER
        }

        private void DeleteCommandExecute(object parameter)
        {
            if (parameter is InventoryDataGridVmDTO dto)
            {
                var items = dto.ProductData;
                var prompt = MessageBox.Show($"Do you really want to remove {items.Count()} items?", "Remove Action", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (prompt == MessageBoxResult.No)
                {
                    dto.DeleteHandle();
                }

                _productService.RemoveMany(items.Select(i => i.Item));


            }
        }
    }
}

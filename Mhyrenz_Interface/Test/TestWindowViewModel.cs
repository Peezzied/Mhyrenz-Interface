using System;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.Test
{
    public class TestWindowViewModel : BaseViewModel
    {
        private readonly IInventoryStore _inventoryStore;
        private readonly IProductService _productService;
        private readonly CreateViewModel<InventoryViewModel> _inventoryViewModel;
        private BaseViewModel _context;
        public BaseViewModel Context
        {
            get
            {
                return _context;
            }
            set
            {
                _context = value;
                OnPropertyChanged(nameof(Context));
            }
        }

        public TestWindowViewModel(IInventoryStore inventoryStore, CreateViewModel<InventoryViewModel> viewModelFactory)
        {
            _inventoryStore = inventoryStore;

            _inventoryViewModel = viewModelFactory;

            Context = _inventoryViewModel();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

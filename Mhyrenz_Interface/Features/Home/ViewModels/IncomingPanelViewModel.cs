using System.Linq;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Home.ViewModels
{
    public class IncomingPanelViewModel : BaseViewModel
    {
        private readonly IInventoryStore _inventoryStore;
        private readonly ISerialBarcodeService _serialBarcodeService;
        private readonly ShellViewModel _mainViewModel;
        private readonly INavigationServiceEx _navigationService;

        public IncomingPanelViewModel(IInventoryStore inventoryStore,
            ISerialBarcodeService serialBarcodeService,
            INavigationServiceEx navigationService,
            ShellViewModel mainViewModel)
        {
            _inventoryStore = inventoryStore;
            _navigationService = navigationService;
            _serialBarcodeService = serialBarcodeService;
            _mainViewModel = mainViewModel;

            _inventoryStore.PropertyChanged += InventoryStore_PropertyChanged;
            _inventoryStore.PurchaseEvent += InventoryStore_PurchaseEvent;
            _serialBarcodeService.OnBarcodeReceived += SerialBarcodeService_OnBarcodeReceived;
        }


        private void SerialBarcodeService_OnBarcodeReceived(string obj)
        {
            //if (!_mainViewModel.CanMainBarcodeReceive)
            //    return;

            //var product = _inventoryStore.GetProductByBarcode(obj);

            //if (product.NetQty == 0)
            //{
            //    Growl.Error($"Unable to complete the operation because of insufficient stock of product \"{product.Name}\"");
            //    return;
            //}

            //TransactionType = "Barcode";
            //_isIncomingScan = true;
            //App.Current.Dispatcher.Invoke(() => _navigationService.Navigate(typeof(HomeView)));

            //_inventoryStore.PurchaseProduct(product,
            //    new Core.TargetChangedEventArgs(product, nameof(ProductDataViewModel.PurchaseDefaultEdit)),
            //    oldValue: product.PurchaseDefaultEdit,
            //    newValue: 1,
            //    tracker: _inventoryStore.GetTrackerByProduct(product),
            //    purchaseProductCommand: _directPurchaseCommand());
        }

        private void InventoryStore_PurchaseEvent(object sender, InventoryStoreEventArgs e)
        {
            TransactionType = "Encode";
            UpdateProduct(e.Product);
        }
        private void InventoryStore_PropertyChanged(object sender, InventoryStoreEventArgs e)
        {
            UpdateProduct(sender.CastTo<ProductDataViewModel>());
        }

        private void UpdateProduct(ProductDataViewModel product)
        {
            Item = product;

            var newTotal = (double)_inventoryStore.Store.Sum(i => i.NetRetailPrice);
            if (newTotal != CurrentTotalPrice)
                LastTotalPrice = CurrentTotalPrice;
            CurrentTotalPrice = newTotal;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Qty));
            OnPropertyChanged(nameof(Price));
        }

        public override void Dispose()
        {
            _inventoryStore.PropertyChanged -= InventoryStore_PropertyChanged;
            _inventoryStore.PurchaseEvent -= InventoryStore_PurchaseEvent;
            _serialBarcodeService.OnBarcodeReceived -= SerialBarcodeService_OnBarcodeReceived;
        }

        private ProductDataViewModel _item;
        public ProductDataViewModel Item
        {
            get => _item;
            set
            {
                _item = value;
                OnPropertyChanged(nameof(Item));
            }
        }

        public string Name => Item.Name;
        public int Qty => Item.Qty;
        public double Price => (double)Item.RetailPrice;

        private double _currentTotalPrice;
        public double CurrentTotalPrice
        {
            get => _currentTotalPrice;
            set
            {
                _currentTotalPrice = value;
                OnPropertyChanged(nameof(CurrentTotalPrice));
            }
        }

        private double _lastTotalPrice;
        public double LastTotalPrice
        {
            get => _lastTotalPrice;
            set
            {
                _lastTotalPrice = value;
                OnPropertyChanged(nameof(LastTotalPrice));
            }
        }
        private string _transactionType;
        public string TransactionType
        {
            get => _transactionType;
            set
            {
                if (_isIncomingScan)
                {
                    _isIncomingScan = false;
                    return;
                }

                _transactionType = value;
                OnPropertyChanged(nameof(TransactionType));
            }
        }

        private bool _isIncomingScan;
    }
}

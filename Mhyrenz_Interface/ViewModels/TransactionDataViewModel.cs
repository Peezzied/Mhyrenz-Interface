using System;
using System.Windows.Input;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;

namespace Mhyrenz_Interface.ViewModels
{
    public class TransactionDataViewModelDTO
    {
        public Guid Id { get; set; }
        public ProductDataViewModel Product { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
        public Session Session { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
    }

    public class TransactionDataViewModel : BaseViewModel, IBarcodeBound
    {
        private readonly IInventoryStore _inventroyStore;
        private readonly INavigationServiceEx _navigationService;
        private readonly ISerialBarcodeService _serialBarcodeService;

        public event Action BarcodeReceived;

        public TransactionDataViewModelDTO DTO { get; set; }
        public TransactionDataViewModel(INavigationServiceEx navigationServiceEx, TransactionDataViewModelDTO dto, IInventoryStore inventroyStore, ISerialBarcodeService serialBarcodeService)
        {
            DTO = dto;

            _inventroyStore = inventroyStore;
            _navigationService = navigationServiceEx;
            _serialBarcodeService = serialBarcodeService;

            _inventroyStore.PropertyChanged += OnProductPropertyChanged;
        }

        public void LoadReceiver()
        {
            _serialBarcodeService.OnBarcodeReceived += SerialBarcodeService_OnBarcodeReceived;
        }

        private void SerialBarcodeService_OnBarcodeReceived(string obj)
        {
            Barcode = obj;
            BarcodeReceived?.Invoke();
        }

        public override void Dispose()
        {
            _serialBarcodeService.OnBarcodeReceived -= SerialBarcodeService_OnBarcodeReceived;
            BarcodeReceived = null;
        }

        private void OnProductPropertyChanged(object sender, InventoryStoreEventArgs e)
        {
            if (e.ProductId != DTO.Product.Item.Id)
                return;

            OnPropertyChanged(nameof(Product));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Price));
            OnPropertyChanged(nameof(Barcode));
        }

        public ICommand GoToItemCommand => Product?.GoToItemCommand;

        public ProductDataViewModel Product => DTO.Product;

        public Session Session => DTO.Session;

        public string Barcode
        {
            get => Product?.Barcode;
            set
            {
                if (Product?.Barcode != value)
                {
                    Product.Barcode = value;
                    OnPropertyChanged(nameof(Barcode));
                }
            }
        }

        public bool IsDeleted => DTO.Product == null;

        public string Category => DTO.Category;

        public string Name => DTO.ProductName;

        public decimal Price => Product?.RetailPrice ?? DTO.Price;

        public int Amount
        {
            get => DTO.Amount;
            set
            {
                if (DTO.Amount != value)
                {
                    //_product = value;
                    DTO.Amount = value;
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }
        public string Date => DTO.Date.ToString("T");
    }
}

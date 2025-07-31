using HandyControl.Tools.Command;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.ViewModels
{
    public class TransactionDataViewModelDTO
    {
        public Guid Id { get; set; }
        public ProductDataViewModel Product { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
        public Session Session { get; set; }
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

        public void Load()
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

        public ICommand GoToItemCommand => DTO.Product.GoToItemCommand;

        public ProductDataViewModel Product
        {
            get => DTO.Product;
            set
            {
                if (DTO.Product != value)
                {
                    //_product = value;
                    DTO.Product = value;

                }
            }
        }

        public string Barcode
        {
            get => DTO.Product.Barcode;
            set
            {
                if (DTO.Product.Barcode != value)
                {
                    DTO.Product.Barcode = value;
                    OnPropertyChanged(nameof(Barcode));
                }
            }
        }
        public string Name
        {
            get => DTO.Product.Name;
        }
        public decimal Price
        {
            get => DTO.Product.RetailPrice;
        }
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
        public string Date
        {
            get => DTO.Date.ToString("T");
        }
    }
}

using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class TransactionDataViewModel: BaseViewModel
    {
        private readonly IInventoryStore _inventroyStore;
        public TransactionDataViewModelDTO DTO { get; set; }
        public TransactionDataViewModel(TransactionDataViewModelDTO dto, IInventoryStore inventroyStore)
        {
            DTO = dto;

            _inventroyStore = inventroyStore;

            _inventroyStore.PropertyChanged += OnProductPropertyChanged;
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

        private ProductDataViewModel _product;
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

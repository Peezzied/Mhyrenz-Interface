using Mhyrenz_Interface.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.ViewModels
{
    public class TransactionDataViewModelDTO
    {
        public ProductDataViewModel Product { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
    }

    public class TransactionDataViewModel: BaseViewModel
    {
        private readonly TransactionDataViewModelDTO _dto;
        public TransactionDataViewModel(TransactionDataViewModelDTO dto)
        {
            _dto = dto;
        }

        public int? Barcode 
        {
            get => _dto.Product.Barcode;
            set
            {
                if (_dto.Product.Barcode != value)
                {
                    _dto.Product.Barcode = value;
                    OnPropertyChanged(nameof(Barcode));
                }
            }
        }
        public string Name 
        {
            get => _dto.Product.Name;
        }
        public decimal Price 
        { 
            get => _dto.Product.RetailPrice;
        }
        public int Amount 
        {
            get => _dto.Amount;
        } 
        public DateTime Date 
        { 
            get => _dto.Date;
        }
    }
}

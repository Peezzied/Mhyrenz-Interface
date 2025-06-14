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
        public Transaction Transaction { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
    }

    public class TransactionDataViewModel: BaseViewModel
    {
        public Transaction Item { get; }

        public TransactionDataViewModel(TransactionDataViewModelDTO dto)
        {
            Item = dto.Transaction;
            Amount = dto.Amount;
            Date = dto.Date;
        }

        public string Barcode { get; set; }
        public string Name 
        {
            get => Item.Item.Name ?? "- -";
        }
        public decimal Price 
        { 
            get => Item.Item.ListPrice;
        }
        public int Amount { get; } 
        public DateTime Date { get; }
    }
}

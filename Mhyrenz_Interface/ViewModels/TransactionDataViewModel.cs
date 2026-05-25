using System;
using System.Windows.Input;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;

namespace Mhyrenz_Interface.ViewModels
{

    public class TransactionDataViewModel : BaseViewModel
    {
        public int Qty { get; set; }

        public string Name { get; set; }

        public decimal RetailPrice { get; set; }

        public decimal TotalPrice => RetailPrice * Qty;
    }
}

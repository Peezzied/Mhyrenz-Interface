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
        public TransactionDataViewModel(Transaction transaction)
        {
            Transaction = transaction;
        }

        private Transaction _transaction;
		public Transaction Transaction
		{
			get => _transaction;
			set
			{
				_transaction = value;
				_qty = _transaction.Amount;
				OnPropertyChanged(null);
			}
		}

		private int _qty;
		public int Qty
		{
			get => _qty;
			set
			{
				_qty = value;
				OnPropertyChanged(nameof(Qty));
			}
		}

		public bool IsPharma => Transaction.Product.IsPharma;

        public bool IsPrescribed { get; set; }

		public Product Product => Transaction.Product; 

        public decimal RetailPrice => Transaction.RetailPrice;

        public decimal TotalPrice => Transaction.LineTotal;
    }
}

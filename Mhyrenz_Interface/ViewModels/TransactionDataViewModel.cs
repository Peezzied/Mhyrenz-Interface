using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Controls.Attached;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;

namespace Mhyrenz_Interface.ViewModels
{

    public class TransactionDataViewModel : TrackedViewModel, IFlashRequestable
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

        public event EventHandler<RowFlashRequestedEventArgs> FlashRequested;

        public Task RequestFlash(SaleBoundPurchaseCommand.DTO.Type type)
        {
            var args = new RowFlashRequestedEventArgs(type);
            FlashRequested?.Invoke(this, args);

            return args.Completion.Task;
        }

		private int _qty;

        public int Qty
		{
			get => _qty;
			set
			{
                SetTrackedProperty(ref _qty, value, nameof(Qty));
                OnPropertyChanged(null);
            }
		}

		public bool IsPharma => Transaction.Product.IsPharma;

        public bool IsPrescribed { get; set; }

		public Product Product => Transaction.Product; 

        public decimal RetailPrice => Transaction.RetailPrice;

        public decimal TotalPrice => Transaction.LineTotal;
    }
}

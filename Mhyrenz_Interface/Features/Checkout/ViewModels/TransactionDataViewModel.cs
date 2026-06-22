using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Checkout.ViewModels
{

    public class TransactionDataViewModel : TrackedViewModel, IFlashRequestable
    {
        public TransactionDataViewModel(Transaction transaction, IInventoryStore inventoryStore)
        {
            Transaction = transaction;
            _inventoryStore = inventoryStore;
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

        public RelayCommand DiscountCommand { get; }

        public event EventHandler<RowFlashRequestedEventArgs> FlashRequested;

        public Task RequestFlash(DataGridFlashBehavior.OperationType type)
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
                if (_qty != value)
                {
                    SetTrackedProperty(ref _qty, value, nameof(Qty));
                    OnPropertyChanged(null);
                }
            }
        }

        private int _qtyIncrementEdit;
        private readonly IInventoryStore _inventoryStore;

        public int QtyIncrementEdit
        {
            get => _qtyIncrementEdit;

            set
            {
                if (_qtyIncrementEdit != value)
                {
                    SetTrackedProperty(ref _qtyIncrementEdit, value, nameof(QtyIncrementEdit));
                    OnPropertyChanged(null);
                }
                _qtyIncrementEdit = 0;
            }
        }

        public Discount Discount
        {
            get => Transaction.Discount;
            set
            {
                SetTrackedProperty(Discount, value,
                    v => Transaction.Discount = v, nameof(Discount));
                OnPropertyChanged(null);
            }
        }

        public int MaxQty => Product.Qty;

        public int MaxIncrementQty => Product.NetQty;

        public bool IsPharma => Product.IsPharma;

        public bool IsPrescribed { get; set; }

        public Product Product => _inventoryStore.Store[Transaction.ProductId].Item;

        public decimal RetailPrice => Transaction.RetailPrice;

        public decimal TotalPrice => Transaction.LineTotal;

        public decimal DiscountAmount => Transaction.DiscountAmount;

        public string DiscountInfo => $"{Discount} Discount ({Transaction.DiscountRate:P0})";

        public bool HasDiscount => Discount != Discount.None;
    }
}

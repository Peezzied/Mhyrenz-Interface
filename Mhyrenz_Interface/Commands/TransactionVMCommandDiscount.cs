using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.TransactionService;
using Mhyrenz_Interface.State;

namespace Mhyrenz_Interface.Commands
{
    public class TransactionVMCommandDiscount : PropertyChangeCommand<TransactionVMRowInfo>
    {
        private readonly DTO _dto;
        private readonly ICheckoutService _checkoutService;
        private readonly ITransactionStore _transactionStore;

        public TransactionVMCommandDiscount(DTO dto, ICheckoutService checkoutService, ITransactionStore transactionStore) : base(dto)
        {
            _dto = dto;
            _checkoutService = checkoutService;
            _transactionStore = transactionStore;
        }

        public override async void Command(object parameter, ActionType intent)
        {
            var discount = (Discount)(intent == ActionType.Undo ? _dto.ChangedArgs.OldValue : _dto.ChangedArgs.NewValue);
            var result = await _checkoutService.ApplyDiscount(new DiscountInfo
            {
                Discount = discount,
                DiscountRate = discount == Discount.None ? 0m : 0.20m // FIXME source the discount rate from user preference
            }, saleId: _dto.SaleId, transactionId: _dto.TransactionId);

            _transactionStore.OnSaleChange(result.Sale);
            await _transactionStore.UpdateTransaction(result.Transaction);
        }

        public new class DTO : PropertyChangeCommand<TransactionVMRowInfo>.DTO
        {
            public int SaleId { get; internal set; }
            public int TransactionId { get; internal set; }
        }
    }
}

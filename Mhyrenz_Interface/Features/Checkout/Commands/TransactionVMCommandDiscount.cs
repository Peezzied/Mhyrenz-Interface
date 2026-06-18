using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.TransactionService;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Checkout.Commands
{
    public class TransactionVMCommandDiscount : PropertyChangeCommand<TransactionVMRowInfo>
    {
        private readonly DTO _dto;
        private readonly ICheckoutService _checkoutService;
        private readonly ITransactionStore _transactionStore;
        private CheckoutResult _result;

        public TransactionVMCommandDiscount(DTO dto, ICheckoutService checkoutService, ITransactionStore transactionStore) : base(dto)
        {
            _dto = dto;
            _checkoutService = checkoutService;
            _transactionStore = transactionStore;
            Completer = CompleterHandler;
        }

        private async Task CompleterHandler(NavigationViewModel navigationViewModel)
        {
            await Complete();
            // TOOD RowIntoView
        }

        public override async Task Command()
        {
            await base.Command();

            var discount = (Discount)(Intent == ActionType.Undo ? _dto.ChangedArgs.OldValue : _dto.ChangedArgs.NewValue);
            _result = await _checkoutService.ApplyDiscount(new DiscountInfo
            {
                Discount = discount,
                DiscountRate = discount == Discount.None ? 0m : 0.20m // FIXME source the discount rate from user preference
            }, saleId: _dto.SaleId, transactionId: _dto.TransactionId);

            if (Intent == ActionType.Normal)
            {
                await Complete();
            }
        }

        private async Task Complete()
        {
            _transactionStore.OnSaleChange(_result.Sale);
            await _transactionStore.UpdateTransaction(_result.Transaction);
        }

        public new class DTO : PropertyChangeCommand<TransactionVMRowInfo>.DTO
        {
            public int SaleId { get; internal set; }
            public int TransactionId { get; internal set; }
        }
    }
}

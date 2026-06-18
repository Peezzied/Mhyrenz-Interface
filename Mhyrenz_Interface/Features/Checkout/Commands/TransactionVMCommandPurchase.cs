using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Checkout.Commands
{
    public class TransactionVMRowInfo
    {
        public int Sale { get; set; }
        public int[] Transactions { get; set; }
    }

    public class TransactionVMCommandPurchase : PropertyChangeCommand<TransactionVMRowInfo>
    {
        private readonly DTO _dto;
        private readonly ICheckoutService _checkoutService;
        private readonly ITransactionStore _transactionStore;
        private Task<CheckoutResult> _result;

        public TransactionVMCommandPurchase(DTO dto, ICheckoutService checkoutService, ITransactionStore transactionStore) : base(dto)
        {
            _dto = dto;
            _checkoutService = checkoutService;
            _transactionStore = transactionStore;
            Completer = CompleterHandler;
        }

        private async Task Complete()
        {
            var result = await _result;
            _transactionStore.AddToSale(result);

            _dto.TransactionId = result.Transaction.Id;
        }

        private async Task CompleterHandler(NavigationViewModel vm)
        {
            await Complete();
            // TODO RowIntoView
        }

        public override async Task Command()
        {
            await base.Command();

            var newValue = PropertyChangedArgs.NewValue as int? ?? 0;
            var oldValue = PropertyChangedArgs.OldValue as int? ?? 0;

            if (newValue == oldValue)
                return;

            var amount = Math.Abs(oldValue - newValue);

            var isIncrease = newValue > oldValue;

            var shouldAdd =
                Intent == ActionType.Undo
                    ? !isIncrease
                    : isIncrease;

            if (shouldAdd)
            {
                _result = _checkoutService.AddItem(_dto.SaleId, _dto.ProductId, amount);
            }
            else
            {
                _result = _checkoutService.Subtract(_dto.SaleId, _dto.TransactionId, amount);
            }

            if (Intent == ActionType.Normal)
            {
                await Complete();
            }
        }

        public new class DTO : PropertyChangeCommand<TransactionVMRowInfo>.DTO
        {
            public int SaleId { get; set; }
            public int ProductId { get; set; }
            public int TransactionId { get; set; }
        }
    }

}

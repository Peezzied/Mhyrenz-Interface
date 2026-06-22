using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Inventory.Commands
{
    public class ProductVMCommandPurchase : ProductVMPropertyChangeCommand
    {
        private readonly DTO _dto;
        private readonly ICheckoutService _checkoutService;
        private readonly ISessionStore _sessionStore;
        private readonly ITransactionStore _transactionStore;
        private Task<CheckoutResult> _result;
        private int _amount;

        public ProductVMCommandPurchase(DTO dto, ICheckoutService checkoutService, ISessionStore sessionStore, ITransactionStore transactionStore) : base(dto)
        {
            _dto = dto;
            _checkoutService = checkoutService;
            _sessionStore = sessionStore;
            _transactionStore = transactionStore;
        }

        protected override async Task CompleterHandler(NavigationViewModel vm)
        {
            await base.CompleterHandler(vm);
            await Complete();
        }

        private async Task Complete()
        {
            _transactionStore.AddToSale(await _result, _amount);
        }

        public override async Task Command()
        {
            await base.Command();

            var newValue = PropertyChangedArgs.NewValue as int? ?? 0;
            var oldValue = PropertyChangedArgs.OldValue as int? ?? 0;

            if (newValue == oldValue)
            {
                Cancel = true;
                return;
            }

            _amount = Math.Abs(newValue - oldValue);
            var isIncrease = newValue > oldValue;

            var shouldAdd =
                Intent == ActionType.Undo
                    ? !isIncrease
                    : isIncrease;

            if (shouldAdd)
            {
                _result = _checkoutService.AddItem(_dto.ProductId, _amount);
            }
            else
            {
                _result = _checkoutService.Subtract(_dto.ProductId, _amount);
            }

            if (Intent == ActionType.Normal)
            {
                await Complete();
            }
        }

        public new class DTO : PropertyChangeCommand<ProductVMRowInfo>.DTO
        {
            public int ProductId { get; set; }
        }
    }
}

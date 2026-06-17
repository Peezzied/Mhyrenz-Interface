using System;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Inventory.Commands
{
    public class ProductVMCommandPurchase : ProductVMPropertyChangeCommand
    {
        private readonly DTO _dto;
        private readonly ICheckoutService _checkoutService;
        private readonly ISessionStore _sessionStore;
        private readonly ITransactionStore _transactionStore;
        private readonly ICheckoutService checkoutService;

        public ProductVMCommandPurchase(DTO dto, ICheckoutService checkoutService, ISessionStore sessionStore, ITransactionStore transactionStore) : base(dto)
        {
            _dto = dto;
            _checkoutService = checkoutService;
            _sessionStore = sessionStore;
            _transactionStore = transactionStore;
        }

        public override async void Command(object parameter, ActionType intent)
        {
            var newValue = PropertyChangedArgs.NewValue as int? ?? 0;
            var oldValue = PropertyChangedArgs.OldValue as int? ?? 0;

            if (newValue == oldValue)
                return;

            var amount = Math.Abs(newValue - oldValue);
            var isIncrease = newValue > oldValue;

            var shouldAdd =
                intent == ActionType.Undo
                    ? !isIncrease
                    : isIncrease;

            CheckoutResult result;
            if (shouldAdd)
            {
                result = await _checkoutService.AddItem(_dto.ProductId, _sessionStore.CurrentSession.Id, amount);
            }
            else
            {
                result = await _checkoutService.Subtract(_dto.ProductId, _sessionStore.CurrentSession.Id, amount);
            }
            _transactionStore.AddToSale(result);
        }

        public new class DTO : PropertyChangeCommand<ProductVMRowInfo>.DTO
        {
            public int ProductId { get; set; }
        }
    }
}

using System;
using System.Windows.Input;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;

namespace Mhyrenz_Interface.Commands
{
    public class ProductVMCommandPurchase : ProductVMPropertyChangeCommand
    {
        private readonly DTO _dto;
        private readonly ICheckoutService _checkoutService;
        private readonly ISessionStore _sessionStore;
        private readonly ICheckoutService checkoutService;
        private readonly ISessionStore sessionStore;

        public ProductVMCommandPurchase(DTO dto, ICheckoutService checkoutService, ISessionStore sessionStore) : base(dto)
        {
            _dto = dto;
            _checkoutService = checkoutService;
            _sessionStore = sessionStore;
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

            if (shouldAdd)
            {
                await _checkoutService.AddItem(_dto.ProductId, _sessionStore.CurrentSession.Id, amount);
            }
            else
            {
                await _checkoutService.Subtract(_dto.ProductId, _sessionStore.CurrentSession.Id, amount);
            }
        }

        public new class DTO : PropertyChangeCommand<ProductVMRowInfo>.DTO
        {
            public int ProductId { get; set; }
        }
    }
}

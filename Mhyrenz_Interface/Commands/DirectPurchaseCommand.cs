using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.State;

namespace Mhyrenz_Interface.Commands
{
    public class DirectPurchaseCommand : BaseAsyncCommand
    {
        private readonly ICheckoutService _checkoutService;
        private readonly ISessionStore _sessionStore;

        public DirectPurchaseCommand(ICheckoutService checkoutService, ISessionStore sessionStore)
        {
            _checkoutService = checkoutService;
            _sessionStore = sessionStore;
        }

        public class DTO
        {
            public enum Type { Add, Subtract }
            public int Amount { get; set; }
            public int ProductId { get; set; }
            public Type Method { get; set; }
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var DTO = parameter as DTO;
            var method = DTO.Method;

            switch (method)
            {
                case DTO.Type.Add:
                    await _checkoutService.AddItem(DTO.ProductId, _sessionStore.CurrentSession.Id, DTO.Amount);
                    break;
                case DTO.Type.Subtract:
                    await _checkoutService.Subtract(DTO.ProductId, _sessionStore.CurrentSession.Id, DTO.Amount);
                    break;
            }
        }
    }
}

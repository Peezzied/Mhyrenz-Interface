using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.TransactionService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.Views;

namespace Mhyrenz_Interface.Commands
{
    public class SaleBoundPurchaseCommand : BaseAsyncCommand
    {
        private readonly ITransactionStore _transactionStore;
        private readonly ICheckoutService _checkoutService;
        private DateTime _dateTime;

        private int _transactionId;

        public SaleBoundPurchaseCommand(ITransactionStore transactionStore, ICheckoutService checkoutService)
        {
            _transactionStore = transactionStore;
            _checkoutService = checkoutService;
        }

        public class DTO
        {
            public enum Type { AddNew, Add, Subtract }
            public int Amount { get; set; }
            public int SaleId { get; set; }
            public int ProductId { get; set; }
            public Type Method { get; set; }
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var DTO = parameter as DTO;

            _dateTime = _dateTime == default ? DateTime.Now : _dateTime;

            switch (DTO.Method)
            {
                case DTO.Type.AddNew:
                case DTO.Type.Add:
                    var result = await _checkoutService.AddItem(DTO.SaleId, DTO.ProductId, DTO.Amount);
                    _transactionStore.AddToSale(result);
                    _transactionId = result.Transaction.Id;
                    break;
                case DTO.Type.Subtract:
                    _transactionStore.AddToSale(await _checkoutService.Subtract(DTO.SaleId, _transactionId, DTO.Amount));
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported method: {DTO.Method}");
            }
        }
    }
}

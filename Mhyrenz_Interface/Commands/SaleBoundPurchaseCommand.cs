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
    public class SaleBoundPurchaseCommand : BaseAsyncCommand, IUndoRedoBound
    {
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly ICheckoutService _checkoutService;
        private DateTime _dateTime;

        public CheckoutResult CheckoutResult { get; private set; }

        private int _transactionId;

        public bool AllowBack { get; private set; } = true;

        public SaleBoundPurchaseCommand(IUndoRedoManager undoRedoManager, ICheckoutService checkoutService)
        {
            _undoRedoManager = undoRedoManager;
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

        public override void Execute(object parameter)
        {
            _undoRedoManager.Push(new UndoRedoBoundCommand(this, null, typeof(CheckoutView), parameter));
            base.Execute(parameter);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var DTO = parameter as DTO;

            _dateTime = _dateTime == default ? DateTime.Now : _dateTime;

            await Execute(DTO);
        }

        private async Task Execute(DTO DTO)
        {
            switch (DTO.Method)
            {
                case DTO.Type.Add:
                    CheckoutResult = await _checkoutService.AddItem(DTO.SaleId, DTO.ProductId, DTO.Amount);
                    _transactionId = CheckoutResult.Transaction.Id;
                    break;
                case DTO.Type.Subtract:
                    CheckoutResult = await _checkoutService.Subtract(DTO.SaleId, _transactionId, DTO.Amount);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported method: {DTO.Method}");
            }
        }

        public async void Undo(object parameter)
        {
            var DTO = parameter as DTO;
            DTO.Method = DTO.Type.Subtract;
            await Execute(DTO);
        }

        public async void Redo(object parameter)
        {
            var DTO = parameter as DTO;
            DTO.Method = DTO.Type.Add;
            await Execute(parameter as DTO);
        }

        public void ExecuteRaw(object parameter)
        {
            base.Execute(parameter);
        }
    }
}

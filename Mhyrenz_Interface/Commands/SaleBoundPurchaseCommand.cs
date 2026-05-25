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
        private DTO.Type _method;
        private DateTime _dateTime;

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
            public SaleTabItem SaleTabItem { get; set; }
            public int SaleId { get; set; }
            public DiscountInfo DiscountInfo { get; set; }
            public int ProductId { get; set; }
            public Type Method { get; set; }
        }

        public override void Execute(object parameter)
        {
            if (_undoRedoManager.Push(new UndoRedoBoundCommand(this, null, typeof(CheckoutView), parameter)))
                base.Execute(parameter);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var DTO = parameter as DTO;
            _method = DTO.Method;

            _dateTime = _dateTime == default ? DateTime.Now : _dateTime;

            await Execute(DTO);
        }

        private async Task Execute(DTO DTO)
        {
            CheckoutResult checkoutResult;
            switch (_method)
            {
                case DTO.Type.AddNew: // TODO when the add is new transactions, apply dateTime to Sale
                case DTO.Type.Add:
                    checkoutResult = await _checkoutService.AddItem(DTO.SaleId, DTO.ProductId, DTO.DiscountInfo, DTO.Amount);
                    break;
                case DTO.Type.Subtract:
                    checkoutResult = await _checkoutService.Subtract(DTO.SaleId, DTO.ProductId, DTO.Amount);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported method: {_method}");
            }

            DTO.SaleTabItem.UpdateSale(checkoutResult);
        }

        public async void Undo(object parameter)
        {
            await Execute(parameter as DTO);
        }

        public async void Redo(object parameter)
        {
            await Execute(parameter as DTO);
        }

        public void ExecuteRaw(object parameter)
        {
            base.Execute(parameter);
        }
    }
}

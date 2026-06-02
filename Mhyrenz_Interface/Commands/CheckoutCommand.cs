using System;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Controls;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.Views;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface.Commands
{
    public class CheckoutCommand : BaseAsyncCommand, IUndoRedoBound
    {
        private readonly int _saleId;
        private readonly ICheckoutService _checkoutService;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly ITransactionStore _transactionStore;

        public bool AllowBack { get; private set; } = true;

        public CheckoutCommand(int saleId, ICheckoutService checkoutService, IUndoRedoManager undoRedoManager, ITransactionStore transactionStore)
        {
            _saleId = saleId;
            _checkoutService = checkoutService;
            _undoRedoManager = undoRedoManager;
            _transactionStore = transactionStore;
        }

        public override void Execute(object parameter = null)
        {
            _undoRedoManager.Push(new UndoRedoBoundCommand(this, null, typeof(InventoryView), parameter));
            base.Execute(parameter);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var result = MessageBox.Show(
                "Do you want to complete this sale?",
                "Complete Sale",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var sale = await _checkoutService.CompleteSale(_saleId);
            await _checkoutService.DiscardSale(_saleId, asComplete: true);

            _transactionStore.OnSaleChange(sale);
        }

        public void ExecuteRaw(object parameter)
        {
            base.Execute(parameter);
        }

        public void Redo(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Undo(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}

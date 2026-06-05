using System;
using System.Threading.Tasks;
using System.Windows;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Inventory.Views;
using Mhyrenz_Interface.Store;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface.Features.Checkout.Commands
{
    public class CheckoutCommand : BaseAsyncCommand, IUndoRedoBound
    {
        private readonly int _saleId;
        private readonly decimal _received;
        private readonly ICheckoutService _checkoutService;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly ITransactionStore _transactionStore;

        public bool AllowBack { get; private set; } = true;

        public CheckoutCommand(int saleId, decimal received, ICheckoutService checkoutService, IUndoRedoManager undoRedoManager, ITransactionStore transactionStore)
        {
            _saleId = saleId;
            _received = received;
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

            var sale = await _checkoutService.CompleteSale(_saleId, _received);

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

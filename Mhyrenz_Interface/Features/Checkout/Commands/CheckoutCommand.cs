using System.Threading.Tasks;
using System.Windows;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Checkout.Views;
using Mhyrenz_Interface.Store;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface.Features.Checkout.Commands
{
    public class CheckoutCommand : UndoRedoBoundCommand
    {
        private readonly int _saleId;
        private readonly decimal _received;
        private readonly ISessionStore _sessionStore;
        private readonly ICheckoutService _checkoutService;
        private readonly ITransactionStore _transactionStore;

        public bool AllowBack { get; private set; } = true;

        public CheckoutCommand(
            int saleId,
            decimal received,
            ISessionStore sessionStore,
            ICheckoutService checkoutService,
            ITransactionStore transactionStore) : base(typeof(CheckoutView))
        {
            _saleId = saleId;
            _received = received;
            _sessionStore = sessionStore;
            _checkoutService = checkoutService;
            _transactionStore = transactionStore;
        }

        public override async Task Command()
        {
            var result = MessageBox.Show(
                "Do you want to complete this sale?",
                "Complete Sale",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var sale = await _checkoutService.CompleteSale(_saleId, _received);

            await _sessionStore.UpdateSession();

            _transactionStore.OnSaleChange(sale);

            App.UndoRedoManager.RemoveAll(c => 
                c is ISaleBoundCommand saleCommand && 
                saleCommand.SaleId == _saleId);
        }
    }
}

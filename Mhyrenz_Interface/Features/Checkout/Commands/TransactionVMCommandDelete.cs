using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Mhyrenz_Interface.Features.Checkout.Views;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Checkout.Commands
{
    public class TransactionVMCommandDelete : UndoRedoBoundCommand, ISaleBoundCommand
    {
        private readonly DTO _dto;
        private readonly IInventoryStore _inventoryStore;
        private readonly ITransactionStore _transactionStore;
        private readonly ICheckoutService _checkoutService;
        private CheckoutResult _result;
        private TransactionVMRowInfo _rowInfo;

        public TransactionVMCommandDelete(DTO dto, IInventoryStore inventoryStore, ITransactionStore transactionStore, ICheckoutService checkoutService) : base(typeof(CheckoutView))
        {
            _dto = dto;
            _inventoryStore = inventoryStore;
            _transactionStore = transactionStore;
            _checkoutService = checkoutService;

            Completer = CompleterHandler;
        }

        private async Task CompleterHandler(BaseViewModel viewModel)
        {
            if (viewModel is CheckoutViewModel checkout
                && checkout.SelectedItem is IFlashRequestable flasher
                && viewModel is IDataGridTabHost host)
            {
                host.RowIntoView(_rowInfo);

                _transactionStore.OnSaleChange(_result.Sale);

                var isDeleted = false;
                async Task apply(Transaction transaction)
                {
                    _inventoryStore.Store.TryGetValue(transaction.ProductId, out var productVm);
                    _transactionStore.Store.TryGetValue(transaction.TransactionKey, out var transactionVm);

                    if (transaction.IsDeleted)
                    {
                        await flasher.RequestFlash(transactionVm, DataGridFlashBehavior.OperationType.Remove);
                        productVm.Purchase -= transaction.Amount;
                    }
                    else
                        productVm.Purchase += transaction.Amount;

                    isDeleted = transaction.IsDeleted;
                }

                await Task.WhenAll(_result.Transactions.Select(t => apply(t)));

                if (isDeleted)
                {
                    _transactionStore.Store.RemoveMany(_result.Transactions.Select(t => t.TransactionKey));
                    return;
                }

                var vms = _transactionStore.AddManyTransactions(_result.Transactions);
                await Task.WhenAll(vms.Select(t => flasher.RequestFlash(t, DataGridFlashBehavior.OperationType.New))); // FIXME does not show. maybe apply some delay

            }
            else
            {
                Cancel = true;
            }
        }

        public int SaleId { get; }

        public override async Task Command()
        {
            if (Intent != ActionType.Undo)
            {
                _result = await _checkoutService.MarkRemoveMany(_dto.SaleId, _dto.Transactions);
            }
            else
            {
                _result = await _checkoutService.MarkRemoveMany(_dto.SaleId, _dto.Transactions, isDeleted: false);
            }

            _rowInfo = new TransactionVMRowInfo
            {
                Sale = _dto.SaleId
            };
        }

        public class DTO
        {
            public int SaleId { get; set; }
            public IEnumerable<int> Transactions { get; set; }
        }
    }
}

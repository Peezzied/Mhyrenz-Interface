using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Mhyrenz_Interface.Features.Checkout.Views;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Checkout.Commands
{
    public class TransactionVMCommandDelete : UndoRedoBoundCommand, ISaleBoundCommand
    {
        private readonly DTO _dto;
        private readonly ITransactionStore _transactionStore;
        private readonly ICheckoutService _checkoutService;

        public TransactionVMCommandDelete(DTO dto, ITransactionStore transactionStore, ICheckoutService checkoutService) : base(typeof(CheckoutView))
        {
            _dto = dto;
            _transactionStore = transactionStore;
            _checkoutService = checkoutService;
        }

        public int SaleId { get; }

        public override async Task Command()
        {
            CheckoutResult result;

            if (Intent != ActionType.Undo)
            {
                result = await _checkoutService.MarkRemoveMany(_dto.SaleId, _dto.Transactions);
            }
            else
            {
                result = await _checkoutService.MarkRemoveMany(_dto.SaleId, _dto.Transactions, isDeleted: false);

            }

            await _transactionStore.RemoveFromSale(result);
        }

        public class DTO
        {
            public int SaleId { get; set; }
            public IEnumerable<int> Transactions { get; set; }
        }
    }
}

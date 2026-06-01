using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;

namespace Mhyrenz_Interface.Commands
{
    public class TransactionVMCommandQty : BaseAsyncCommand
    {
        private readonly ICheckoutService _checkoutService;

        public TransactionVMCommandQty(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var transaction = parameter as Transaction;
            await _checkoutService.Update(transaction.Id, transaction);
        }
    }
}

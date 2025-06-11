using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Commands
{
    public class PurchaseProductCommand : BaseAsyncCommand
    {
        private readonly ITransactionsService _transactionsService;
        private readonly IInventroyStore _inventroyStore;

        public PurchaseProductCommand(ITransactionsService transactionsService, IInventroyStore inventroyStore)
        {
            _transactionsService = transactionsService;
            _inventroyStore = inventroyStore;
        }

        public override Task ExecuteAsync(object parameter)
        {
            
        }
    }
}

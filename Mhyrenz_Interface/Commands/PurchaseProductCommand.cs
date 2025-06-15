using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Exceptions;
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
        private readonly IInventoryStore _inventroyStore;

        public PurchaseProductCommand(ITransactionsService transactionsService, IInventoryStore inventroyStore)
        {
            _transactionsService = transactionsService;
            _inventroyStore = inventroyStore;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var DTO = parameter as PurchaseProductDTO;
            try
            {
                switch (DTO.Method)
                {
                    case PurchaseProductDTO.Type.Add:
                        await _transactionsService.Add(DTO.Product, DTO.Amount);
                        break;
                    case PurchaseProductDTO.Type.Remove:
                        await _transactionsService.Remove(DTO.Product, DTO.Amount);
                        break;
                }
            }
            catch (InsufficientQuantityException)
            {

                throw;
            }
            catch (NegativeException)
            {
                throw;
            }
        }
    }
}

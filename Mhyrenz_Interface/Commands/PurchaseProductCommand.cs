using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Exceptions;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
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
        private DateTime _dateTime;

        public PurchaseProductCommand(ITransactionsService transactionsService, IInventoryStore inventroyStore)
        {
            _transactionsService = transactionsService;
            _inventroyStore = inventroyStore;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var DTO = parameter as PurchaseProductDTO;
            var intent = DTO.Intent;
            var method = DTO.Method;

            _dateTime = DateTime.Now;

            try
            {

                if (intent == PropertyChangeCommand<ProductDataViewModel>.ActionType.Undo)
                {
                    switch (method)
                    {
                        case PurchaseProductDTO.Type.Add:
                            await _transactionsService.Remove(DTO.Product, DTO.Amount);
                            break;
                        case PurchaseProductDTO.Type.Remove:
                            await _transactionsService.Add(DTO.Product, _dateTime, DTO.Amount);
                            break;
                    }
                }
                else
                {
                    switch (method)
                    {
                        case PurchaseProductDTO.Type.Add:
                            await _transactionsService.Add(DTO.Product, _dateTime, DTO.Amount);
                            break;
                        case PurchaseProductDTO.Type.Remove:
                            await _transactionsService.Remove(DTO.Product, DTO.Amount);
                            break;
                    }
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

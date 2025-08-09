using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Exceptions;
using Mhyrenz_Interface.Domain.Models;
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
        private readonly bool _canCombine;
        private DateTime _dateTime;

        public PurchaseProductCommand(ITransactionsService transactionsService, IInventoryStore inventroyStore, bool canCombine = false)
        {
            _transactionsService = transactionsService;
            _inventroyStore = inventroyStore;
            _canCombine = canCombine;
        }

        public class DTO
        {
            public enum Type { Add, Remove }
            public int Amount { get; set; }
            public Product Product { get; set; }
            public Type Method { get; set; }
            public ActionType Intent { get; internal set; }
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var DTO = parameter as DTO;
            var method = DTO.Method;

            _dateTime = _dateTime == default ? DateTime.Now : _dateTime;

            switch (method)
            {
                case DTO.Type.Add:
                    await _transactionsService.Add(DTO.Product, _dateTime, DTO.Amount, _canCombine);
                    break;
                case DTO.Type.Remove:
                    await _transactionsService.Remove(DTO.Product, DTO.Amount);
                    break;
            }
        }
    }
}

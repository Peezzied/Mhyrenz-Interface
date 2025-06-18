using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Commands
{
    public class SalesRegisterCommand : BaseAsyncCommand
    {
        private readonly ISalesRecordService _salesRecordService;
        private readonly ITransactionStore _transactionStore;
        private readonly ISessionService _sessionService;
        private readonly ISessionStore _sessionStore;
        public SalesRegisterCommand(
            ISalesRecordService salesRecordService,
            ISessionService sessionService,
            ITransactionStore transactionStore,
            ISessionStore sessionStore)
        {
            _salesRecordService = salesRecordService;
            _sessionService = sessionService;
            _transactionStore = transactionStore;
            _sessionStore = sessionStore;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var transactions = _transactionStore.Transactions;

            var session = await _sessionService.GenerateSession(new Session { Period = DateTime.Now });

            var sales = new SalesRecord()
            {
                SessionId = session.UniqueId,
                TotalPurchase = transactions.Sum(t => t.Amount),
                TotalSales = (double)transactions.Sum(t => t.DTO.Product.Item.NetRetail), // REFACTOR TRANSACTION MODELTYPES decimal TO double
                Profit = (double)transactions.Sum(t => t.DTO.Product.Item.Profit), // REFACTOR TRANSACTION MODELTYPES decimal TO double
                RegisteredAt = session.Period
            };

            _sessionStore.CurrentSession = session;
            await _salesRecordService.RegisterSales(sales);
        }
    }
}

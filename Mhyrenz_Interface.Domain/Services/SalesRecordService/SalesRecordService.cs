using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Domain.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services.SalesRecordService
{
    public class SalesRecordService : ISalesRecordService
    {
        private readonly ISalesRecordDataService _salesRecordDataService;
        private readonly ITransactionsService _transactionsService;
        public SalesRecordService(ISalesRecordDataService salesRecordDataService, ITransactionsService transactionsService)
        {
            _salesRecordDataService = salesRecordDataService;
            _transactionsService = transactionsService;
        }
        public async Task<bool> RegisterSales(SalesRecord sales)
        {
            await _transactionsService.RemoveAll();
            await _salesRecordDataService.Create(sales);

            return true;
        }
    }
}

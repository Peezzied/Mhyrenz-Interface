using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services.SalesRecordService
{
    [Obsolete]
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
            await Task.Run(() =>
            {
                _transactionsService.RemoveAll();
                _salesRecordDataService.Create(sales);
            });

            return true;
        }
    }
}

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
        public SalesRecordService(ISalesRecordDataService salesRecordDataService)
        {
            _salesRecordDataService = salesRecordDataService;
        }
        public async Task<bool> RegisterSales(SalesRecord sales)
        {
            await _salesRecordDataService.Create(sales);

            return true;
        }
    }
}

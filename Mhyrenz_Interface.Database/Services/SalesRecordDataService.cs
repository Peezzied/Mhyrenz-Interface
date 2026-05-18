using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Database.Services
{
    [Obsolete]

    public class SalesRecordDataService : ISalesRecordDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;
        public SalesRecordDataService(InventoryDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Task<SalesRecord> Create(SalesRecord entity)
        {
            throw new NotImplementedException();
        }

        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<SalesRecord> Update(int id, SalesRecord updatedEntity)
        {
            throw new NotImplementedException();
        }
    }
}

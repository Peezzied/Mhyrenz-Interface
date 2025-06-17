using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Database.Services
{
    public class SalesRecordDataService : GenericDataService<SalesRecord>, ISalesRecordDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;
        public SalesRecordDataService(InventoryDbContextFactory contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override async Task<SalesRecord> Get(int id)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                SalesRecord entity = await context.SalesRecords
                    .Include(a => a.Session)
                    .FirstOrDefaultAsync((e) => e.Id == id);
                return entity;
            }
        }

        public override async Task<IEnumerable<SalesRecord>> GetAll()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                var entity = await context.SalesRecords
                    .Include(a => a.Session)
                    .ToListAsync();
                return entity;
            }
        }
    }
}

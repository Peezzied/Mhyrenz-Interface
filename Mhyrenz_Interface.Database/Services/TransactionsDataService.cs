using Mhyrenz_Interface.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Database.Services
{
    public class TransactionsDataService: GenericDataService<Transaction>, ITransactionsDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;

        public TransactionsDataService(InventoryDbContextFactory contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override async Task<IEnumerable<Transaction>> GetAll()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                IEnumerable<Transaction> entity = await context.Transactions
                    .Include(a => a.Item)
                    .ToListAsync();
                return entity;

            }
        }
    }
}

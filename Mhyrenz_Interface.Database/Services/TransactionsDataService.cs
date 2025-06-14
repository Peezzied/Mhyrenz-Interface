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

        public async Task<IEnumerable<Transaction>> GetLatestsByProduct(int productId)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                IEnumerable<Transaction> entity = await context.Transactions
                    .Include(a => a.Item)
                    .Where(t => t.ProductId == productId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
                return entity;
            }
        }

        public async Task<Transaction> GetLast()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                return await context.Transactions
                    .AsNoTracking()
                    .Include(t => t.Item)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<IEnumerable<Transaction>> GetLatests()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                IEnumerable<Transaction> entity = await context.Transactions
                    .Include(a => a.Item)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
                return entity;

            }
        }
    }
}

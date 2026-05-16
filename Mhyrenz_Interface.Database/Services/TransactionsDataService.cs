using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Transaction = Mhyrenz_Interface.Domain.Models.Transaction;

namespace Mhyrenz_Interface.Database.Services
{
    public class TransactionsDataService : ITransactionsDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;

        public TransactionsDataService(InventoryDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Clean()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                await context.Database.ExecuteSqlRawAsync($"DELETE FROM sqlite_sequence WHERE name = '{nameof(context.Transactions)}';");
                await context.Database.ExecuteSqlRawAsync("VACUUM;");
            }
        }

        public async Task<IReadOnlyList<Transaction>> GetAll()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                return await LoadTransactions(context)
                    .IgnoreQueryFilters()
                    .OrderByDescending(t => t.Timestamp)
                    .ToListAsync();
            }
        }

        public async Task<IReadOnlyList<Transaction>> GetAllByProduct(int productId)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                return await LoadTransactions(context)
                    .IgnoreQueryFilters()
                    .Where(t => t.ProductId == productId)
                    .OrderByDescending(t => t.Timestamp)
                    .ToListAsync();
            }
        }

        public async Task<Transaction> GetLast()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                return await LoadTransactions(context)
                    .IgnoreQueryFilters()
                    .OrderByDescending(t => t.Timestamp)
                    .FirstOrDefaultAsync();
            }
        }

        private static Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Transaction, Session> LoadTransactions(InventoryDbContext context)
        {
            return context.Transactions
                .Include(a => a.Item)
                .Include(a => a.Session);
        }

        public async Task<IReadOnlyList<Transaction>> CreateMany(IEnumerable<Transaction> entities)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var result = entities.ToList();
                await context.Transactions.AddRangeAsync(result);
                await context.SaveChangesAsync();
                return result;
            }
        }

        public async Task DeleteMany(IEnumerable<Transaction> entities)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.Transactions.RemoveRange(entities);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Transaction> Get(int id)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                return await LoadTransactions(context)
                    .FirstOrDefaultAsync((e) => e.Id == id); ;
            }
        }
    }
}

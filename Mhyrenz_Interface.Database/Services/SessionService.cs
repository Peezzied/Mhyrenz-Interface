using System;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Domain.Services.SessionService
{
    public class SessionService : ISessionService
    {
        private readonly InventoryDbContextFactory _inventoryDbContextFactory;
        private readonly ICheckoutService _checkoutService;
        private readonly IProductService _productService;
        private readonly IDatabaseSnapshotService _databaseSnapshotService;

        public SessionService(InventoryDbContextFactory inventoryDbContextFactory, ICheckoutService checkoutService, IProductService productService, IDatabaseSnapshotService databaseSnapshotService)
        {
            _inventoryDbContextFactory = inventoryDbContextFactory;
            _checkoutService = checkoutService;
            _productService = productService;
            _databaseSnapshotService = databaseSnapshotService;
        }

        public async Task DeleteSession(Guid id)
        {

            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var entity = await context.Sessions
                    .FirstAsync(s => s.Id == id);

                await _databaseSnapshotService.ExportSnapshot(entity, isBackup: true);

                context.Sessions.Remove(entity);

                await ResetSession(context, await context.Sundry.FirstOrDefaultAsync());

                await context.SaveChangesAsync();
            }
        }

        public async Task<Session> GenerateSession(Session session)
        {
            using (InventoryDbContext context = _inventoryDbContextFactory.CreateDbContext())
            {
                context.Sessions.Add(session);

                session.Code = Session.GenerateCode(session.Id, session.Period);

                await context.SaveChangesAsync();

                return session;
            }
        }

        public async Task<Session> EditSession(Guid id, DateTime period)
        {
            using (InventoryDbContext context = _inventoryDbContextFactory.CreateDbContext())
            {
                var entity = await context.Sessions
                    .FirstAsync((e) => e.Id == id);

                entity.Period = period;
                entity.Code = Session.GenerateCode(id, period);

                context.Sessions.Update(entity);
                await context.SaveChangesAsync();
                return entity;
            }
        }

        public async Task RecordSession()
        {
            using (InventoryDbContext context = _inventoryDbContextFactory.CreateDbContext())
            {
                await _productService.ApplyPurchases();

                await _productService.RemovePhysically();

                var session = await context.Sessions
                    .FirstAsync();

                await _checkoutService.ConvertAgnosticTransactions();

                await _databaseSnapshotService.ExportSnapshot(session);

                session.IsActive = false;

                var sundry = await context.Sundry.FirstOrDefaultAsync();

                // Fix: project columns, aggregate client-side.
                var transactionData = await context.Transactions
                    .AsNoTracking()
                    .Select(t => new { t.RetailPrice, t.CostPrice, t.Amount })
                    .ToListAsync();

                var salesData = await context.Sales
                    .AsNoTracking()
                    .Select(s => s.Total)
                    .ToListAsync();

                var profit = transactionData.Sum(t => (t.RetailPrice - t.CostPrice) * t.Amount)
                           + (sundry?.Profit ?? 0);
                var sales = salesData.Sum() + (sundry?.Sales ?? 0);
                var salesCount = salesData.Count;

                await context.SalesRecords.AddAsync(new SalesRecord
                {
                    SessionId = session.Id,
                    SundryProfit = sundry?.Profit ?? 0,
                    SundrySales = sundry?.Sales ?? 0,
                    Profit = profit,
                    Sales = sales,
                    SalesCount = salesCount
                });

                await ResetSession(context, sundry);

                await context.SaveChangesAsync();
            }
        }

        public async Task<Session> GetSession()
        {
            using (InventoryDbContext context = _inventoryDbContextFactory.CreateDbContext())
            {
                var entity = await context.Sessions
                    .Include(s => s.Sales)
                    .FirstOrDefaultAsync();
                return entity;
            }
        }

        private static async Task ResetSession(InventoryDbContext context, Sundry sundry)
        {
            if (sundry != null)
                context.Sundry.Remove(sundry);

            context.RemoveRange(context.Transactions);
            context.RemoveRange(context.Sales);

            await context.Database.ExecuteSqlRawAsync(
                "DELETE FROM sqlite_sequence WHERE name IN ({0}, {1})",
                nameof(context.Transactions),
                nameof(context.Sales)
            );
        }
    }
}

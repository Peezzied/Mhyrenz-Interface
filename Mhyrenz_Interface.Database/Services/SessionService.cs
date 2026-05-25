using System;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Domain.Services.SessionService
{
    public class SessionService : ISessionService
    {
        private readonly InventoryDbContextFactory _inventoryDbContextFactory;

        public SessionService(InventoryDbContextFactory inventoryDbContextFactory)
        {
            _inventoryDbContextFactory = inventoryDbContextFactory;
        }

        public async Task DeleteSession(Guid id)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var entity = await context.Sessions
                    .FirstOrDefaultAsync(s => s.Id == id);
                context.Sessions.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Session> GenerateSession(Session session)
        {
            using (InventoryDbContext context = _inventoryDbContextFactory.CreateDbContext())
            {
                context.Sessions.Add(session);
                await context.SaveChangesAsync();

                return session;
            }
        }

        public async Task<Session> EditSession(Guid id, DateTime period)
        {
            using (InventoryDbContext context = _inventoryDbContextFactory.CreateDbContext())
            {
                var entity = await context.Sessions
                    .FirstOrDefaultAsync((e) => e.Id == id);
                entity.Period = period;
                context.Sessions.Update(entity);
                await context.SaveChangesAsync();
                return entity;
            }
        }

        public async Task<Session> GetSession()
        {
            using (InventoryDbContext context = _inventoryDbContextFactory.CreateDbContext())
            {
                var entity = await context.Sessions
                    .Include(s => s.Sales)
                    .OrderByDescending(s => s.Period)
                    .FirstOrDefaultAsync();
                return entity;
            }
        }
    }
}

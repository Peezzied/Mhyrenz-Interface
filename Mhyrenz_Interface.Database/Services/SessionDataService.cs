using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Database.Services
{
    public class SessionDataService : ISessionDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;
        private readonly ITransactionsDataService _transactionsDataService;

        public SessionDataService(InventoryDbContextFactory contextFactory, ITransactionsDataService transactionsDataService)
        {
            _contextFactory = contextFactory;
            _transactionsDataService = transactionsDataService;
        }

        public async Task<Session> Create(Session entity)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                var result = await context.Sessions.AddAsync(entity);
                await context.SaveChangesAsync();

                return result.Entity;
            }
        }

        public async Task Delete(Guid id)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                var entity = await LoadSessions(context)
                    .FirstOrDefaultAsync((e) => e.Id == id);
                context.Sessions.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Session> Get(Guid id)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                return await LoadSessions(context)
                    .FirstOrDefaultAsync((e) => e.Id == id); ;
            }
        }

        public async Task<IReadOnlyList<Session>> GetAll()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                return await LoadSessions(context)
                    .ToListAsync(); ;
            }
        }

        public async Task<Session> Update(Guid id, Session updatedEntity)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                updatedEntity.Id = id;

                context.Sessions.Update(updatedEntity);
                await context.SaveChangesAsync();

                return updatedEntity;
            }
        }

        private static Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Session, IEnumerable<Transaction>> LoadSessions(InventoryDbContext context)
        {
            return context.Sessions
                .Include(a => a.Transactions);
        }
    }
}

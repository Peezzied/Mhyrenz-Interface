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
    public class SessionDataService: ISessionDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;

        public SessionDataService(InventoryDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Session> Update(Guid id, Session updatedEntity)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                updatedEntity.UniqueId = id;

                context.Set<Session>().Update(updatedEntity);
                await context.SaveChangesAsync();

                return updatedEntity;
            }
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

        public async Task<bool> Delete(Guid uid)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                Session entity = await context.Sessions.FirstOrDefaultAsync((e) => e.UniqueId == uid);
                context.Sessions.Remove(entity);
                await context.SaveChangesAsync();

                return true;
            }
        }

        public async Task<Session> Get(Guid uid)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                Session entity = await context.Sessions
                    .Include(e => e.Transactions)
                    .FirstOrDefaultAsync((e) => e.UniqueId == uid);
                return entity;
            }
        }

        public async Task<IEnumerable<Session>> GetAll()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                IEnumerable<Session> entities = await context.Sessions
                    .Include(e => e.Transactions)
                    .ToListAsync();
                return entities;
            }
        }
    }
}

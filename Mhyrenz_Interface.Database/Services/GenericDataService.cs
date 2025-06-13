using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Database.Services
{
    public class GenericDataService<T> : IDataService<T> where T : DomainObject
    {
        private readonly InventoryDbContextFactory _contextFactory;

        public GenericDataService(InventoryDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<T> Create(T entity)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {

                var result = await context.Set<T>().AddAsync(entity);
                await context.SaveChangesAsync();

                return result.Entity;
            }
        }

        public async Task CreateMany(IEnumerable<T> entities)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.Set<T>().AddRange(entities);
                await context.SaveChangesAsync();
            }
        }


        public async Task<bool> Delete(int id)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                T entity = await context.Set<T>().FirstOrDefaultAsync((e) => e.Id == id);
                context.Set<T>().Remove(entity);
                await context.SaveChangesAsync();

                return true;
            }
        }

        public async Task DeleteMany(IEnumerable<T> entities)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.Set<T>().RemoveRange(entities);
                await context.SaveChangesAsync();
            }
        }

        public async virtual Task<T> Get(int id)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                T entity = await context.Set<T>().FirstOrDefaultAsync((e) => e.Id == id);
                return entity;
            }
        }

        public async virtual Task<IEnumerable<T>> GetAll()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                IEnumerable<T> entities = await context.Set<T>().ToListAsync();
                return entities;
            }
        }


        public async Task<T> Update(int id, T updatedEntity)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                updatedEntity.Id = id;

                context.Set<T>().Update(updatedEntity);
                await context.SaveChangesAsync();

                return updatedEntity;
            }
        }

        public async Task<T> UpdateProperty(
            int id, string propertyName, object newValue)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                var entity = await context.Set<T>().FindAsync(id);
                if (entity == null)
                    return null;

                var property = typeof(T).GetProperty(propertyName);
                if (property == null || !property.CanWrite)
                    throw new InvalidOperationException($"'{propertyName}' is not a valid property of {typeof(T).Name}");

                var convertedValue = Convert.ChangeType(newValue, property.PropertyType);
                property.SetValue(entity, convertedValue);

                await context.SaveChangesAsync();
                return entity;
            }

        }


    }
}

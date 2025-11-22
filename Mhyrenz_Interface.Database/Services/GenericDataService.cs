using EFCore.BulkExtensions;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Database.Services
{
    public class GenericDataService<T> : IDataService<T> where T : DomainObject
    {
        public string Name = nameof(T).TableName();
        private readonly InventoryDbContextFactory _contextFactory;

        public GenericDataService(InventoryDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<T> Create(T entity)
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    GetTable().Insert(entity);
                    return entity;
                }
            });
        }

        public async Task<IEnumerable<T>> CreateMany(IEnumerable<T> entities)
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    GetTable().InsertBulk(entities);
                    return entities;
                }
            });
            
        }


        public async Task Delete(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                await Task.Run(() => GetTable().Delete(id));
            }
        }

        public async Task DeleteMany(IEnumerable<T> entities)
        {
            var ids = entities.Select(i => i.Id).ToHashSet();
            using (var context = _contextFactory.CreateDbContext())
            {
                await Task.Run(() => GetTable().DeleteMany(i => ids.Contains(i.Id)));
            }
        }

        public async virtual Task<T> Get(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await Task.Run(() => GetTable().FindById(id));
            }
        }

        public async virtual Task<IEnumerable<T>> GetAll()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await Task.Run(() => GetTable().FindAll());
            }
        }


        public async Task Update(int id, T updatedEntity)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                await Task.Run(() => GetTable().Update(id, updatedEntity));
            }
        }

        public async Task<T> UpdateProperty(int id, string propertyName, object newValue)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await Task.Run(() =>
                {
                    var entity = GetTable().FindById(id);
                    GetTable().Update(UpdateEntityProperty(entity, propertyName, newValue));

                    return entity;
                });
            }

        }

        public async Task<IEnumerable<T>> UpdatePropertyRange(IEnumerable<T> entities, string propertyName, object newValue)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var ids = entities.Select(i => i.Id).ToHashSet();
                return await Task.Run(() =>
                {
                    IEnumerable<T> newEntities = entities.Select(i =>
                    {
                        var edited = UpdateEntityProperty(i, propertyName, newValue);
                        GetTable().Update(edited);
                        return edited;
                    }).ToList();

                    return newEntities;
                });
            }
        }

        protected ILiteCollection<T> GetTable()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return context.GetCollection<T>(Name);
            }
        }

        private T UpdateEntityProperty(T entity, string propertyName, object newValue)
        {
            if (entity == null)
                return null;

            var property = typeof(T).GetProperty(propertyName);
            if (property == null || !property.CanWrite)
                throw new InvalidOperationException($"'{propertyName}' is not a valid property of {typeof(T).Name}");

            Type targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            var convertedValue = Convert.ChangeType(newValue, targetType);
            property.SetValue(entity, convertedValue);

            return entity;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;

namespace Mhyrenz_Interface.Database.Services
{
    public class GenericDataService<T> : IDataService<T> where T : DomainObject
    {
        public string Name = typeof(T).TableName();
        private readonly InventoryDbService _context;

        public GenericDataService(InventoryDbService context)
        {
            _context = context;
        }

        public async Task<T> Create(T entity)
        {
            return await Task.Run(() =>
            {
                GetTable().Insert(entity);
                return entity;
            });
        }

        public async Task<IEnumerable<T>> CreateMany(IEnumerable<T> entities)
        {
            return await Task.Run(() =>
            {
                GetTable().InsertBulk(entities);
                return entities;
            });

        }


        public async Task Delete(int id)
        {
            await Task.Run(() => GetTable().Delete(id));
        }

        public async Task DeleteMany(IEnumerable<T> entities)
        {
            var ids = entities.Select(i => i.Id).ToHashSet();
            await Task.Run(() => GetTable().DeleteMany(i => ids.Contains(i.Id)));
        }

        public async virtual Task<T> Get(int id)
        {
            return await Task.Run(() => GetTable().FindById(id));
        }

        public async virtual Task<IEnumerable<T>> GetAll()
        {
            return await Task.Run(() => GetTable().FindAll());
        }


        public async Task Update(int id, T updatedEntity)
        {
            await Task.Run(() => GetTable().Update(id, updatedEntity));
        }

        public async Task<T> UpdateProperty(int id, string propertyName, object newValue)
        {
            return await Task.Run(() =>
            {
                var entity = GetTable().FindById(id);
                GetTable().Update(UpdateEntityProperty(entity, propertyName, newValue));

                return entity;
            });

        }

        public async Task<IEnumerable<T>> UpdatePropertyRange(IEnumerable<T> entities, string propertyName, object newValue)
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

        protected ILiteCollection<T> GetTable()
        {
            return _context.Instance.GetCollection<T>(Name);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;

namespace Mhyrenz_Interface.Database.Services
{
    public class GenericDataService<T> : TableBound<T>, IDataService<T> where T : DomainObject
    {
        private readonly InventoryDbService _context;

        public GenericDataService(InventoryDbService context) : base(context)
        {
            _context = context;
        }

        public virtual T Create(T entity)
        {
            GetTable().Insert(entity);
            return entity;
        }

        public virtual IEnumerable<T> CreateMany(IEnumerable<T> entities)
        {
            GetTable().InsertBulk(entities);
            return entities;
        }


        public virtual bool Delete(object id)
        {
            return GetTable().Delete((dynamic)id);
        }

        public virtual void DeleteMany(IEnumerable<T> entities)
        {
            var baseId = typeof(DomainObject).GetProperty("Id"); // base Id to ignore

            var idProp = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p =>
                    p.CanRead && p.CanWrite &&             // must be readable/writable
                    (p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                     p.Name.Equals(typeof(T).Name + "Id", StringComparison.OrdinalIgnoreCase)) &&
                    p != baseId) ?? throw new InvalidOperationException($"Cannot find a derived Id property for type {typeof(T).Name}");

            var ids = entities.Select(i => idProp.GetValue(i)).ToHashSet();
            GetTable().DeleteMany(i => ids.Contains(i.Id));
        }

        public virtual T Get(object id)
        {
            return GetTable().FindById((dynamic)id);
        }

        public virtual IEnumerable<T> GetAll()
        {
            return GetTable().FindAll();
        }


        public virtual T Update(object id, T updatedEntity)
        {
            GetTable().Update((dynamic)id, updatedEntity);
            return Get((dynamic)id);
        }

        public virtual T UpdateProperty(dynamic id, string propertyName, object newValue)
        {
            var entity = GetTable().FindById(id);
            GetTable().Update(UpdateEntityProperty(entity, propertyName, newValue));

            return entity;
        }

        public virtual IEnumerable<T> UpdatePropertyRange(IEnumerable<T> entities, string propertyName, object newValue)
        {
            var ids = entities.Select(i => i.Id).ToHashSet();

            IEnumerable<T> newEntities = entities.Select(i =>
            {
                var edited = UpdateEntityProperty(i, propertyName, newValue);
                GetTable().Update(edited);
                return edited;
            }).ToList();

            return newEntities;
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

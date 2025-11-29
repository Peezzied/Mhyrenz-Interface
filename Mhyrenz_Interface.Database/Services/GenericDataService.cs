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

        public virtual T UpdateProperty(dynamic id, UpdateEntity<T> update)
        {
            var entity = GetTable().FindById(id);
            update(entity);
            GetTable().Update(entity);

            return entity;
        }

        public virtual IEnumerable<T> UpdatePropertyRange(IEnumerable<T> entities, UpdateEntity<T> update)
        {
            var ids = entities.Select(i => i.Id).ToHashSet();

            IEnumerable<T> newEntities = entities.Select(i =>
            {
                update(i);
                GetTable().Update(i);
                return i;
            }).ToList();

            return newEntities;
        }
    }
}

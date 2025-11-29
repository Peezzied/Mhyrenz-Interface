using System.Collections.Generic;

namespace Mhyrenz_Interface.Domain.Services
{
    public delegate void UpdateEntity<T>(T entity);
    public interface IDataService<T>
    {
        IEnumerable<T> GetAll();

        T Get(object id);

        T Create(T entity);

        IEnumerable<T> CreateMany(IEnumerable<T> entities);

        void DeleteMany(IEnumerable<T> entities);

        T Update(object id, T entity);

        T UpdateProperty(object id, UpdateEntity<T> update);
        IEnumerable<T> UpdatePropertyRange(IEnumerable<T> entities, UpdateEntity<T> update);

        bool Delete(object id);
    }
}

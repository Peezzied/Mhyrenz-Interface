using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface IDataService<T>
    {
        Task<IEnumerable<T>> GetAll();

        Task<T> Get(int id);

        Task<T> Create(T entity);

        Task CreateMany(IEnumerable<T> entities);

        Task<T> Update(int id, T entity);

        Task<T> UpdateProperty(int id, string propertyName, object newValue);

        Task<bool> Delete(int id);
    }
}

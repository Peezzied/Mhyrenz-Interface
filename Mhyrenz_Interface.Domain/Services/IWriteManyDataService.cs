using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public delegate void UpdateEntity<T>(T entity);
    public interface IWriteManyDataService<T>
    {
        Task<IReadOnlyList<T>> CreateMany(IEnumerable<T> entities);
        Task DeleteMany(IEnumerable<T> entities);
    }
}

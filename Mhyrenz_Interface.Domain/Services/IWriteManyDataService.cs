using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    [Obsolete("No longer used", true)]
    public delegate void UpdateEntity<T>(T entity);
    [Obsolete("No longer used", true)]
    public interface IWriteManyDataService<T>
    {
        Task<IReadOnlyList<T>> CreateMany(IEnumerable<T> entities);
        Task DeleteMany(IEnumerable<T> entities);
        Task<IReadOnlyList<T>> UpdateMany(IEnumerable<T> entities);
    }
}

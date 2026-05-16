using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface IWriteDataService<T, I>
    {
        Task<T> Create(T entity);
        Task<T> Update(I id, T updatedEntity);
        Task Delete(I id);
    }
}

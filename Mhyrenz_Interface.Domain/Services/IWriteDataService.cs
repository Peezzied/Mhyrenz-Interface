using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    [Obsolete("No longer used", true)]
    public interface IWriteDataService<T, I>
    {
        Task<T> Create(T entity);
        Task<T> Update(T updatedEntity);
        Task Delete(I id);
    }
}

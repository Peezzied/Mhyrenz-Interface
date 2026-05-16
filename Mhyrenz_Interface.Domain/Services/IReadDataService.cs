using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface IReadDataService<T, I>
    {
        Task<IReadOnlyList<T>> GetAll();
        Task<T> Get(I id);
    }
}

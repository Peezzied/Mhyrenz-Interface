using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ISalesDataService : IWriteDataService<Sale, int>, IReadDataService<Sale, int>
    {
        Task<IReadOnlyList<Sale>> GetHistory();
        Task<IReadOnlyList<Sale>> GetActive();
    }
}

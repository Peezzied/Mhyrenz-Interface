using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ISessionDataService : IWriteDataService<Session, Guid>, IReadDataService<Session, Guid>
    {
        Task<Session> GetCurrent();
    }
}

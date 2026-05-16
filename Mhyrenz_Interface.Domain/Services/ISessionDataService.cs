using System;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ISessionDataService : IWriteDataService<Session, Guid>, IReadDataService<Session, Guid>
    {

    }
}

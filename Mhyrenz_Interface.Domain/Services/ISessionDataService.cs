using Mhyrenz_Interface.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ISessionDataService
    {
        Task<Session> Create(Session entity);
        Task<bool> Delete(Guid uid);
        Task<Session> Get(Guid uid);
        Task<IEnumerable<Session>> GetAll();
    }
}

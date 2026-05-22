using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services.SessionService
{
    public interface ISessionService
    {
        Task<Session> GenerateSession(Guid id);
        Task DeleteSession(Guid uid);
        Task<Session> GetSession();
        Task<Session> EditSession(Guid id, DateTime period);
    }
}
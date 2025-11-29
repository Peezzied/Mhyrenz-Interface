using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services.SessionService
{
    public interface ISessionService
    {
        Task<Session> GenerateSession(Session session);
        Task<bool> DeleteSession(Guid uid);
        Task<Session> GetSession();
        Task<Session> EditSession(Guid id, Session session);
    }
}
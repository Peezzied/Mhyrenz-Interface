using Mhyrenz_Interface.Domain.Models;
using System;
using System.Threading.Tasks;

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
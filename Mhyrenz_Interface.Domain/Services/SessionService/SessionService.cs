using System;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services.SessionService
{
    public class SessionService : ISessionService
    {
        private readonly ISessionDataService _sessionDataService;
        public SessionService(ISessionDataService sessionDataService)
        {
            _sessionDataService = sessionDataService;
        }

        public async Task DeleteSession(Guid uid)
        {
            await _sessionDataService.Delete(uid);
        }

        public async Task<Session> GenerateSession(Session session)
        {
            return await _sessionDataService.Create(session);
        }

        public async Task<Session> EditSession(Session session)
        {
            return await _sessionDataService.Update(session);
        }

        public async Task<Session> GetSession()
        {
            return await _sessionDataService.GetCurrent();
        }
    }
}

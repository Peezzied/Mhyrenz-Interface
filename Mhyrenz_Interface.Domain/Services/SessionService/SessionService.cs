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

        public async Task<bool> DeleteSession(Guid uid)
        {
            return await Task.Run(() => _sessionDataService.Delete(uid));
        }

        public async Task<Session> GenerateSession(Session session)
        {
            return await Task.Run(() => _sessionDataService.Create(session));
        }

        public async Task<Session> EditSession(Guid id, Session session)
        {
            return await Task.Run(() => _sessionDataService.Update(id, session));
        }

        public async Task<Session> GetSession()
        {
            return await Task.Run(() =>
            {
                var result = _sessionDataService.GetAll();

                return result.OrderByDescending(s => s.Period).FirstOrDefault();
            });
        }
    }
}

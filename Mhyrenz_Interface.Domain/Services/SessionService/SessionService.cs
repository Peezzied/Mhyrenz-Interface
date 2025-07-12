using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return await _sessionDataService.Delete(uid);
        }

        public async Task<Session> GenerateSession(Session session)
        {
            var result = await _sessionDataService.Create(session);
                        
            return result;
        }

        public async Task<Session> EditSession(Guid id, Session session)
        {
            return await _sessionDataService.Update(id, session);
        }

        public async Task<Session> GetSession() 
        {
            var result = await _sessionDataService.GetAll();

            return result.OrderByDescending(s => s.Period).FirstOrDefault();
        }
    }
}

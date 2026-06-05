using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SessionService;

namespace Mhyrenz_Interface.Store
{
    public class SessionStore : ISessionStore
    {
        public SessionStore(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        private Session _currentSession;
        private readonly ISessionService _sessionService;

        public Session CurrentSession
        {
            get => _currentSession;
            set
            {
                _currentSession = value;
                StateChanged?.Invoke(_currentSession);
            }
        }

        public async Task<Session> UpdateSession()
        {
            CurrentSession = await _sessionService.GetSession();
            return CurrentSession;
        }

        public event Action<Session> StateChanged;
    }
}

using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.State
{
    public class SessionStore : ISessionStore
    {
        private Session _currentSession;
        public Session CurrentSession 
        { 
            get => _currentSession; 
            set
            {
                _currentSession = value;
                StateChanged?.Invoke();
            }
        }

        public event Action StateChanged;
    }
}

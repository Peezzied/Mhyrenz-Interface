using Mhyrenz_Interface.Domain.Models;
using System;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.State
{
    public interface ISessionStore
    {
        Session CurrentSession { get; set; }
        event Action<Session> StateChanged;

        Task<Session> UpdateSession();
    }
}
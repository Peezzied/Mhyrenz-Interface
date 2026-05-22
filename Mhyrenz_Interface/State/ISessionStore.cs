using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.State
{
    public interface ISessionStore
    {
        Session CurrentSession { get; set; }
        event Action<Session> StateChanged;

        Task<Session> UpdateSession();
    }
}
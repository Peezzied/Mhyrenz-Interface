using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Store
{
    public interface ISessionStore
    {
        Session CurrentSession { get; set; }
        event Action<Session> SessionChanged;

        Task<Session> UpdateSession();
    }
}
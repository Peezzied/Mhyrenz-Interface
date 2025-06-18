using Mhyrenz_Interface.Domain.Models;
using System;

namespace Mhyrenz_Interface.Domain.State
{
    public interface ISessionStore
    {
        Session CurrentSession { get; set; }
        event Action StateChanged;
    }
}
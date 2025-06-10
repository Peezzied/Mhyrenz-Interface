using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.State.Mediator
{
    public interface IMediator
    {
        void Register<TMessage>(Action<TMessage> handler);
        void Unregister<TMessage>(Action<TMessage> handler);
        void Send<TMessage>(TMessage message);
    }

}

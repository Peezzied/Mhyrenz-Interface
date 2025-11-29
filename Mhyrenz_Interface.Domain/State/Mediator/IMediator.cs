using System;

namespace Mhyrenz_Interface.Domain.State.Mediator
{
    public interface IMediator
    {
        void Register<TMessage>(Action<TMessage> handler);
        void Unregister<TMessage>(Action<TMessage> handler);
        void Send<TMessage>(TMessage message);
    }

}

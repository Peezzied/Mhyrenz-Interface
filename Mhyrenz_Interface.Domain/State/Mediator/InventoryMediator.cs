using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.State.Mediator
{
    public class GenericMediator : IMediator
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Register<TMessage>(Action<TMessage> handler)
        {
            var type = typeof(TMessage);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();

            _handlers[type].Add(handler);
        }

        public void Unregister<TMessage>(Action<TMessage> handler)
        {
            var type = typeof(TMessage);
            if (_handlers.ContainsKey(type))
            {
                _handlers[type].Remove(handler);
                if (_handlers[type].Count == 0)
                    _handlers.Remove(type);
            }
        }

        public void Send<TMessage>(TMessage message)
        {
            var type = typeof(TMessage);
            if (_handlers.ContainsKey(type))
            {
                foreach (var handler in _handlers[type].OfType<Action<TMessage>>())
                {
                    handler(message);
                }
            }
        }
    }

}

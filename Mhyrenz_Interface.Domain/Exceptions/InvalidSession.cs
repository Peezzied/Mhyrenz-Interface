using Mhyrenz_Interface.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Exceptions
{
    public class InvalidSession : Exception
    {
        private readonly Session session;
        public InvalidSession(Session session)
        {
            this.session = session;
        }

        public InvalidSession(string message, Session session) : base(message)
        {
            this.session = session;
        }

        public InvalidSession(string message, Exception innerException, Session session) : base(message, innerException)
        {
            this.session = session;
        }

    }
}

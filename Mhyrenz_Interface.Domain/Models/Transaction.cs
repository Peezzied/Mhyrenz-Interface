using System;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Transaction : DomainObject
    {
        public Guid UniqueId { get; set; }

        public int ProductId { get; set; }
        public Product Item { get; set; }

        public DateTime Timestamp { get; set; }

        // Session
        public Guid SessionId { get; set; }
        public Session Session { get; set; }

        //public Transaction()
        //{
        //    UniqueId = Guid.NewGuid();
        //    CreatedAt = DateTime.Now;
        //}
    }
}

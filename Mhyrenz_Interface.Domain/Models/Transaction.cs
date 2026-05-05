using System;
using DocumentFormat.OpenXml.Office.CustomUI;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Transaction : DomainObject
    {

        public Guid UniqueId { get; set; }

        public bool IsDeleted => Item == null;

        public decimal Price { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }

        public int ProductId { get; set; }
        public Product Item { get; set; }

        public DateTime Timestamp { get; set; }

        // Session
        public Guid SessionId { get; set; }
        public Session Session { get; set; }
    }
}

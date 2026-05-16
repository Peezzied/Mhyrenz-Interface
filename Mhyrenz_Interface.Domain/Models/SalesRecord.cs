using System;
using System.Collections.Generic;

namespace Mhyrenz_Interface.Domain.Models
{
    [Obsolete]
    public class SalesRecord
    {
        public int Id { get; set; }
        public int TotalPurchase { get; set; }
        public double TotalSales { get; set; }
        public double Profit { get; set; }
        public DateTime RegisteredAt { get; set; }

        public IEnumerable<Sale> Sales { get; set; }

        // Session
        public Guid SessionId { get; set; }
        public Session Session { get; set; }
    }
}

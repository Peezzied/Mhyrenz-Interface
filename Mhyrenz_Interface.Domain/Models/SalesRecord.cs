using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class SalesRecord: DomainObject
    {
        public int TotalPurchase { get; set; }
        public double TotalSales { get; set; }  
        public double Profit { get; set; }
        public DateTime RegisteredAt { get; set; }

        // Session
        public Guid SessionId { get; set; }
        public Session Session { get; set; }
    }
}

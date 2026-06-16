using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class SalesRecord
    {
        public int Id { get; set; }

        public Guid SessionId { get; set; }

        /// <summary>
        /// Sales with sundy.
        /// </summary>
        public decimal Sales { get; set; }
        /// <summary>
        /// Profit with sundy.
        /// </summary>
        public decimal Profit { get; set; }
        public decimal SundryProfit { get; set; }
        public decimal SundrySales { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int SalesCount { get; set; }
    }
}

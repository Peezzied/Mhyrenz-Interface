using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models.Snapshots
{
    public class SaleSnapshot
    {
        public int Id { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public decimal Paid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Completed_at { get; set; }

        // TODO include discount
    }
}

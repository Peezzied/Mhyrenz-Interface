using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models.Snapshots
{
    public class ProductSnapshot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal RetailPrice { get; set; }
        public int Qty { get; set; }
        public string Batch { get; set; }
        public DateTime? Expiry { get; set; }
        public decimal CostPrice { get; set; }
        public string Barcode { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }

        // TODO include PharmaDetails and Supplier(s)
    }
}

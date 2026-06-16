using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models.Snapshots
{
    public class TransactionSnapshot
    {
        public int ProductId { get; set; }
        public int? SaleId { get; set; }
        public int Amount { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal CostPrice { get; set; }
        public decimal DiscountRate { get; set; }

        // TODO include Discount
    }
}

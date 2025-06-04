using Mhyrenz_Interface.Domain.Models.Primatives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Product : IProduct
    {
        public string Name { get; set; }
        //public string Supplier { get; set; } // for later
        public int Qty { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal ListPrice { get; set; }
        public int Barcode { get; set; }
        public Transactions Purchase { get; set; } // Transaction
        public DateTime Expiry { get; set; }
        public string Batch { get; set; }
        public int Category { get; set; }

        // Calculated
        public int NetQty => Qty - Purchase.Count;
        public decimal NetRetail => Purchase.Count * RetailPrice;
        public decimal CostPrice => Qty * RetailPrice;
        public decimal ProfitRevenue => RetailPrice - ListPrice;
        public decimal Profit => Purchase.Count * ProfitRevenue;
        public decimal TotalListPrice => ListPrice * Qty;
    }
}

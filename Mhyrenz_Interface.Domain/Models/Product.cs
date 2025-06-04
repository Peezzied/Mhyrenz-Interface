using Mhyrenz_Interface.Domain.Models.Primatives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public string Supplier { get; set; } // for later
        public int Qty { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal ListPrice { get; set; }
        public int Barcode { get; set; }
        public DateTime Expiry { get; set; }
        public string Batch { get; set; }

        // Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Transaction
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        [NotMapped]
        public Transactions Purchase => new Transactions(Transactions);

        // Calculated
        public int NetQty => Qty - Purchase.Count;
        public decimal NetRetail => Purchase.Count * RetailPrice;
        public decimal CostPrice => Qty * RetailPrice;
        public decimal ProfitRevenue => RetailPrice - ListPrice;
        public decimal Profit => Purchase.Count * ProfitRevenue;
        public decimal TotalListPrice => ListPrice * Qty;

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Models
{
    public class Product
    {
        public int Qty { get; }
        public decimal RetailPrice { get; }
        public decimal ListPrice { get; }
        public int Id { get; }
        public int Purchase { get; } // Transaction
        public DateTime Expiry { get; }
        public string Batch { get; }
        public int Category { get; }

        // Calculated
        public int NetQty => Qty - Purchase;
        public decimal NetRetail => Purchase * RetailPrice;
        public decimal CostPrice => Qty * RetailPrice;
        public decimal ProfitRevenue => RetailPrice - ListPrice;
        public decimal Profit => Purchase * ProfitRevenue;
        public decimal TotalListPrice => ListPrice * Qty;

        public Product(int category, int id, int qty, int purchase, int retailPrice, int listPrice, DateTime expiry, string batch)
        {
            Category = category;
            Id = id;
            Qty = qty;
            Purchase = purchase;
            RetailPrice = retailPrice;
            ListPrice = listPrice;
            Expiry = expiry;
            Batch = batch;
            Category = category;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Product: DomainObject
    {
        public Product() { }
        public Product(string name, decimal retailPrice, decimal listPrice, int categoryId)
        {
            Name = name;
            RetailPrice = retailPrice;
            ListPrice = listPrice;
            CategoryId = categoryId;
        }

        public string Name { get; set; }
        public string GenericName { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public int Qty { get; set; } = 0;
        public decimal RetailPrice { get; set; }
        public decimal ListPrice { get; set; }
        public string Barcode { get; set; }
        public DateTime? Expiry { get; set; }
        public string Batch { get; set; }

        public bool IsDeleted { get; set; }

        public Dictionary<string, object> Extras { get; set; }

        // Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Transaction
        public ICollection<Transaction> Transactions { get; set; }
        public int Purchase => Transactions?.Count ?? 0;

        // Calculated
        public int NetQty => Qty - Purchase;
        public decimal NetRetail => Purchase * RetailPrice;
        public decimal CostPrice => Qty * RetailPrice;
        public decimal ProfitRevenue => RetailPrice - ListPrice;
        public decimal Profit => Purchase * ProfitRevenue;
        public decimal TotalListPrice => ListPrice * Qty;

        public Product Clone()
        {
            return new Product()
            {
                Name = this.Name,
                RetailPrice = this.RetailPrice,
                ListPrice = this.ListPrice,
                Barcode = this.Barcode,
                Expiry = this.Expiry,
                Batch = this.Batch,
                CategoryId = this.CategoryId,
                Qty = this.Qty
            };
        }
    }
}

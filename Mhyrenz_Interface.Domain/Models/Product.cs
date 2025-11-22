using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

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

        // Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Transaction
        public ICollection<Transaction> Transactions { get; set; }
        [BsonIgnore] public int Purchase => Transactions?.Count ?? 0;

        // Calculated
        [BsonIgnore] public int NetQty => Qty - Purchase;
        [BsonIgnore] public decimal NetRetail => Purchase * RetailPrice;
        [BsonIgnore] public decimal CostPrice => Qty * RetailPrice;
        [BsonIgnore] public decimal ProfitRevenue => RetailPrice - ListPrice;
        [BsonIgnore] public decimal Profit => Purchase * ProfitRevenue;
        [BsonIgnore] public decimal TotalListPrice => ListPrice * Qty;

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

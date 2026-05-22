using System;
using System.Collections.Generic;
using System.Linq;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Product
    {

        public int Id { get; set; }
        public string Name { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public int Qty { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal ListPrice { get; set; }
        public string Barcode { get; set; }
        public DateTime? Expiry { get; set; }
        public string Batch { get; set; }

        public bool IsDeleted { get; private set;  }

        public int? PharmaDetailsId { get; set; }
        public PharmaDetails PharmaDetails { get; set; }

        // Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Transaction
        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();
        public int Purchase => Transactions?.Sum(t => t.Amount) ?? 0;

        // Calculated
        public int NetQty => Qty - Purchase;
        public decimal NetRetail => Purchase * RetailPrice;
        public decimal CostPrice => Qty * RetailPrice;
        public decimal ProfitRevenue => RetailPrice - ListPrice;
        public decimal Profit => Purchase * ProfitRevenue;
        public decimal TotalListPrice => ListPrice * Qty;

        public void Delete()
        {
            IsDeleted = true;
        }

        public void DeleteBack()
        {
            IsDeleted = false;
        }

        public void AddItem(int amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Amount must be greater than zero.");

            var existing = Transactions.FirstOrDefault(t => t.ProductId == Id);
            if (existing == null)
            {
                Transactions.Add(new Transaction
                {
                    ProductId = Id,
                    Amount = amount,
                    RetailPrice = RetailPrice
                });
                return;
            }
            
            existing.IncreaseAmount(amount);
        }

        public void SubtractItem(int amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Amount must be greater than zero.");

            var existing = Transactions.FirstOrDefault(t => t.ProductId == Id);
            if (existing == null)
            {
                Transactions.Add(new Transaction
                {
                    ProductId = Id,
                    Amount = -amount,
                    RetailPrice = RetailPrice
                });
                return;
            }

            existing.DecreaseAmount(amount);
        }
    }
}

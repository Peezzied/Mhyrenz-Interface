using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        public decimal CostPrice { get; set; }
        public string Barcode { get; set; }
        public DateTime? Expiry { get; set; }
        public string Batch { get; set; }

        public bool IsDeleted { get; private set; }

        public int? PharmaDetailsId { get; set; }
        public PharmaDetails PharmaDetails { get; set; }

        public bool IsPharma => PharmaDetails != null;

        // Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Transaction
        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();

        [NotMapped]
        public int Purchase { get; set; }

        // Calculated
        public int NetQty => Qty - Purchase;
        public decimal NetRetail => Purchase * RetailPrice;
        //public decimal CostPrice => Qty * RetailPrice;
        public decimal LineCost => CostPrice * Qty;

        public void Delete()
        {
            IsDeleted = true;
        }

        public void DeleteBack()
        {
            IsDeleted = false;
        }

        public void ApplyPurchase(int purchase)
        {
            Qty -= purchase;
        }

        public Transaction AddItem(int amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Amount must be greater than zero.");

            var transaction = Transactions.FirstOrDefault(t => t.ProductId == Id);
            if (transaction == null)
            {
                transaction = new Transaction
                {
                    ProductId = Id,
                    Amount = amount,
                    RetailPrice = RetailPrice,
                    CostPrice = CostPrice
                };
                Transactions.Add(transaction);
                return transaction;
            }

            transaction.IncreaseAmount(amount);

            return transaction;
        }

        public Transaction SubtractItem(int amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Amount must be greater than zero.");

            var existing = Transactions.FirstOrDefault(t => t.ProductId == Id);
            if (existing == null)
            {
                var transaction = new Transaction
                {
                    ProductId = Id,
                    Amount = -amount,
                    RetailPrice = RetailPrice
                };
                Transactions.Add(transaction);
                return transaction;
            }

            existing.DecreaseAmount(amount);

            if (existing.Amount == 0)
                Transactions.Remove(existing);
            
            return existing;
        }

        public void RecalculatePurchase()
        {
            Purchase = Transactions.Sum(t => t.Amount);
        }
    }
}

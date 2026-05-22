using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Report.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using Mhyrenz_Interface.Domain.Services.TransactionService;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime? Completed_at { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public decimal Change { get; set; }
        public ICollection<Transaction> Transactions { get; private set; }
            = new List<Transaction>();

        public Guid SessionId { get; set; }
        public Session Session { get; set; }

        public Transaction AddItem(Product product, DiscountInfo discountInfo, int amount = 1)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Amount must be greater than zero.");
            
            var transaction = Transactions.FirstOrDefault(t =>
                t.ProductId == product.Id &&
                t.Discount == discountInfo.Discount &&
                t.DiscountRate == discountInfo.DiscountRate);

            if (transaction != null)
            {
                transaction.IncreaseAmount(amount);
            }
            else
            {
                transaction = new Transaction
                {
                    ProductId = product.Id,
                    Amount = amount,
                    RetailPrice = product.RetailPrice,
                    Discount = discountInfo.Discount,
                    DiscountRate = discountInfo.DiscountRate
                };
                Transactions.Add(transaction);
            }

            RecalculateTotals();
            return transaction;
        }

        public void ReceiveCash(decimal cashReceived)
        {
            if (cashReceived < 0)
                throw new InvalidOperationException("Cash received cannot be negative.");

            RecalculateTotals();
            Change = cashReceived - Total;
        }

        public void RecalculateTotals()
        {
            SubTotal = Transactions.Sum(t => t.GetSubTotal());
            Total = Transactions.Sum(t => t.GetLineTotal());
        }

        public Transaction SubtractItem(Transaction transaction, int amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Amount must be greater than zero.");

            transaction = Transactions.FirstOrDefault(t => t.Id == transaction.Id) 
                ?? throw new InvalidOperationException("Transaction not found in sale.");

            transaction.DecreaseAmount(amount);

            if (transaction.Amount == 0)
            {
                Transactions.Remove(transaction);
                return null;
            }
            RecalculateTotals();
            return transaction;
        }
    }
}

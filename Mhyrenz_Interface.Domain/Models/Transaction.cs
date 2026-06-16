using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using DocumentFormat.OpenXml.Office.CustomUI;
using Mhyrenz_Interface.Domain.Services.TransactionService;

namespace Mhyrenz_Interface.Domain.Models
{
    public enum Discount
    {
        None,
        PWD,
        Senior
    }
    public class Transaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        /// <summary>
        /// Empty SaleId is populated  once the session has ended.
        /// </summary>
        /// <remarks>
        /// All Transactions with empty SaleId will be collected in a single Sale once the session has ended.
        /// This allows us to keep track of all transactions that occurred during a session, even if they were not immediately associated with a sale at the time of the transaction
        /// created from direct purchase edit in the Inventory. This is to maintain inclusivity and a consistent Sale auditing and reporting.
        /// </remarks>
        public int? SaleId { get; set; }

        public Sale Sale { get; set; }

        public Guid SessionId { get; set; }
        //public Session Session { get; set; }

        public int Amount { get; set; }

        /// <summary>
        /// Retail price snapshot with discount.
        /// </summary>
        public decimal RetailPrice { get; set; }
        /// <summary>
        /// Cost price snapshot.
        /// </summary>
        public decimal CostPrice { get; set; }
        public Discount Discount { get; set; } = Discount.None;

        /// <summary>
        /// Percentage in decimal form (from 0 to 1).
        /// </summary>
        public decimal DiscountRate { get; set; }

        /// <summary>
        /// Total without discount.
        /// </summary>
        public decimal SubTotal => RetailPrice * (1 + DiscountRate) * Amount;

        public decimal DiscountAmount => SubTotal - LineTotal;

        /// <summary>
        /// Total with discount.
        /// </summary>
        public decimal LineTotal => RetailPrice * Amount;

        [NotMapped]
        public long TransactionKey => CreateTransactionKey(ProductId, SaleId);

        public void ApplyDiscount(decimal discountRate)
        {
            DiscountRate = discountRate;
            RetailPrice *= (1 - DiscountRate);
        }

        public void IncreaseAmount(int amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Amount must be greater than zero.");

            Amount += amount;
        }

        internal void DecreaseAmount(int amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Amount must be greater than zero.");

            Amount -= amount;
        }

        public static long CreateTransactionKey(int productId, int? saleId)
        {
            return ((long)(saleId ?? 0) << 32) | (uint)productId;
        }

        public static readonly Expression<Func<Transaction, decimal>>
            ProfitExpression =
                t => (t.RetailPrice - t.CostPrice) * t.Amount;

        public static decimal CalculateProfit(decimal retail, decimal cost, int amount)
        {
            return (retail - cost) * amount;
        }
    }
}

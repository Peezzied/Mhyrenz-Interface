using System;
using System.ComponentModel.DataAnnotations.Schema;
using DocumentFormat.OpenXml.Office.CustomUI;

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
        public Session Session { get; set; }

        public int Amount { get; set; }
        public decimal RetailPrice { get; set; }
        public Discount Discount { get; set; } = Discount.None;

        /// <summary>
        /// Percentage in decimal form (from 0 to 1).
        /// </summary>
        public decimal DiscountRate { get; set; }

        public decimal SubTotal => RetailPrice * Amount;

        public decimal DiscountAmount => SubTotal * DiscountRate;

        public decimal LineTotal => SubTotal - DiscountAmount;

        [NotMapped]
        public long TransactionKey => CreateTransactionKey(ProductId, SaleId);

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
    }
}

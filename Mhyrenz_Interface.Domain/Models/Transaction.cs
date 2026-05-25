using System;
using DocumentFormat.OpenXml.Office.CustomUI;

namespace Mhyrenz_Interface.Domain.Models
{
    public enum Discount
    {
        None,
        Student,
        PWD,
        Senior,
        Custom
    }
    public class Transaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Item { get; set; }

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
        public Discount Discount { get; set; }

        /// <summary>
        /// Percentage in decimal form (from 0 to 1).
        /// </summary>
        public decimal DiscountRate { get; set; }

        public decimal GetSubTotal()
        {
            return RetailPrice * Amount;
        }

        public decimal GetDiscountAmount()
        {
            return GetSubTotal() * DiscountRate;
        }

        public decimal GetLineTotal()
        {
            return GetSubTotal() - GetDiscountAmount();
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
    }
}

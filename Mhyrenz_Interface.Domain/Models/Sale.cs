using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime? Completed_at { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public decimal Paid { get; set; }

        public Discount Discount { get; set; }

        public ICollection<Transaction> Transactions { get; private set; }
            = new List<Transaction>();

        [NotMapped]
        public IEnumerable<Transaction> ActiveTransactions =>
            Transactions.Where(t => !t.IsDeleted);

        public void ReceiveCash(decimal cashReceived)
        {
            if (cashReceived < 0)
                throw new InvalidOperationException("Cash received cannot be negative.");

            RecalculateTotals();
            Paid = cashReceived - Total;
        }

        public void RecalculateTotals(bool isFiltered = true)
        {
            var tx = isFiltered ? ActiveTransactions.ToList() : Transactions;
            SubTotal = tx.Sum(t => t.SubTotal);
            Total = tx.Sum(t => t.LineTotal);
        }

        public string GetCustomerName()
        {
            return $"{Id:D3} {(Discount == Discount.None ? "Regular" : Discount.ToString())} Customer";
        }
    }
}

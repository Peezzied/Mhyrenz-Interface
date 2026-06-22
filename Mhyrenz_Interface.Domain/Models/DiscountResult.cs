using System.Collections.Generic;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Database.Services
{
    public class DiscountResult 
    {
        public IReadOnlyList<Transaction> Transactions { get; set; }

        public Sale Sale { get; set; }
    }
}

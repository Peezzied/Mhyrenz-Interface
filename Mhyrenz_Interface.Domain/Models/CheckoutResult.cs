using System;
using System.Collections.Generic;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Database.Services
{
    public class CheckoutResult
    {
        /// <summary>
        /// If null, transaction ceased to exist because its quantity was reduced to zero.
        /// </summary>
        public Transaction Transaction { get; set; }

        public Sale Sale { get; set; }

        [Obsolete]
        public bool WasRemoved { get; set; } = false;
        public IReadOnlyList<Transaction> Transactions { get; set; }
    }
}

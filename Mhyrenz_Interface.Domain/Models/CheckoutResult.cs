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

        public int? TransactionId { get; set; }

        public bool WasRemoved => Transaction == null && TransactionId.HasValue;
    }
}

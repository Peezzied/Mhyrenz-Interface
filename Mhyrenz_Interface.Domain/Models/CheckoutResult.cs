using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Database.Services
{
    public class CheckoutResult
    {
        /// <summary>
        /// If null, transaction cease to exist (quantity reduced to zero).
        /// </summary>
        public Transaction Transaction { get; set; }
        public Sale Sale { get; set; }
    }
}

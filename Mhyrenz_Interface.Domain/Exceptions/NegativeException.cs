using System;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Exceptions
{
    public class NegativeException : Exception
    {
        public NegativeException(int amount, Product product)
        {
            Amount = amount;
            Product = product;
        }

        public NegativeException(string message, int amount, Product product) : base(message)
        {
            Amount = amount;
            Product = product;
        }

        public NegativeException(string message, Exception innerException, int amount, Product product) : base(message, innerException)
        {
            Amount = amount;
            Product = product;
        }

        public int Amount { get; set; }
        public Product Product { get; set; }
    }
}

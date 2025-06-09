using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Exceptions
{
    public class InvalidPriceException : Exception
    {
        public InvalidPriceException(decimal listPrice, decimal retailPrice)
        {
            ListPrice = listPrice;
            RetailPrice = retailPrice;
        }

        public InvalidPriceException(string message, decimal listPrice, decimal retailPrice) : base(message)
        {
            ListPrice = listPrice;
            RetailPrice = retailPrice;
        }

        public InvalidPriceException(string message, Exception innerException, decimal listPrice, decimal retailPrice) : base(message, innerException)
        {
            ListPrice = listPrice;
            RetailPrice = retailPrice;
        }

        public decimal ListPrice { get; set; }
        public decimal RetailPrice { get; set; }
    }
}

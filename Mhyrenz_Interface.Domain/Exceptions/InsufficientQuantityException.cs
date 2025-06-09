using Mhyrenz_Interface.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Exceptions
{
    public class InsufficientQuantityException: Exception
    {
        public InsufficientQuantityException(decimal qty, decimal netQty, Product product)
        {
            Qty = qty;
            NetQty = netQty;
            Product = product;
        }

        public InsufficientQuantityException(string message, decimal qty, decimal netQty, Product product) : base(message)
        {
            Qty = qty;
            NetQty = netQty;
            Product = product;
        }

        public InsufficientQuantityException(string message, Exception innerException, decimal qty, decimal netQty, Product product) : base(message, innerException)
        {
            Qty = qty;
            NetQty = netQty;
            Product = product;
        }

        public decimal Qty { get; set; }
        public decimal NetQty { get; set; }
        public Product Product { get; set; }
    }
}

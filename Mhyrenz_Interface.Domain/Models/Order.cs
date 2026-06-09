using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int Qty { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public void DecrementQty(int amount)
        {
            Qty -= amount;
        }

        public void IncrementQty(int amount)
        {
            Qty += amount;
        }
    }
}

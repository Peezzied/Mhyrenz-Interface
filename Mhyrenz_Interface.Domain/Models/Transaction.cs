using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Item { get; set; }
    }
}

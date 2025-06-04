using Mhyrenz_Interface.Domain.Models.Primatives;
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
        public IProduct Item { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Transaction: DomainObject
    {
        public Guid UniqueId { get; set; }
        public int ProductId { get; set; }
        public Product Item { get; set; }
        public DateTime CreatedAt { get; set; }

        //public Transaction()
        //{
        //    UniqueId = Guid.NewGuid();
        //    CreatedAt = DateTime.Now;
        //}
    }
}

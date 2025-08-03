using System.Collections.Generic;
using System.ComponentModel;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Supplier: DomainObject
    {
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
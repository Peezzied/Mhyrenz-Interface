using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Category: DomainObject
    {
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}

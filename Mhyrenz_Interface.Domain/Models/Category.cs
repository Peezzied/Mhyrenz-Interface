using System.Collections.Generic;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Category : DomainObject
    {
        public new int Id { get; set; }
        public string Name { get; set; }
        //public bool IsDeleted { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}

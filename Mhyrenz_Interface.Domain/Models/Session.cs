using System;
using System.Collections.Generic;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public DateTime Period { get; set; }
        public IEnumerable<Sale> Sales { get; set; }
    }
}

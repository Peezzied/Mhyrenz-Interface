using System;
using System.Collections.Generic;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public DateTime Period { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public IEnumerable<Sale> Sales { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        /// <summary>
        /// Business date of session.
        /// </summary>
        public DateTime Period { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public IEnumerable<Sale> Sales { get; set; }

        public static string GenerateCode(Guid id, DateTime period)
        {
            return $"MHR-{period:yyyyMMdd}-{id:N}";
        }
    }
}

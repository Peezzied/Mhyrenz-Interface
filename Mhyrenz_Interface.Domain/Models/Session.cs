using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class Session
    {
        public Guid UniqueId { get; set; }
        public DateTime Period { get; set; }
    }
}

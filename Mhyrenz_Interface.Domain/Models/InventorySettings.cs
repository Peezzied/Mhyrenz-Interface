using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class InventorySettings
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public bool IdColumn { get; set; }
        public bool BatchColumn { get; set; }
        public bool ExpiryDateColumn { get; set; }
        public bool SupplierColumn { get; set; }

        public bool? GenericColumn { get; set; }
    }
}

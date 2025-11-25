using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models
{
    public class InventorySettings
    {
        public class ColumnSchema
        {
            public string Name { get; set; }
            public string Field { get; set; }
            public string Type { get; set; }
            public bool Visible { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IdColumn { get; set; }
        public bool BatchColumn { get; set; }
        public bool ExpiryDateColumn { get; set; }
        public bool SupplierColumn { get; set; }
        public List<ColumnSchema> ColumnExtras { get; set; }
    }
}

using System.Collections.Generic;

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

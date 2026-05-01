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
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public List<ColumnSchema> ExtraColumns { get; set; }
    }
}

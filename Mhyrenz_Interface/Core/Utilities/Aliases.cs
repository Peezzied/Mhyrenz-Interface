using System;
using System.Collections.Generic;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Core.Utilities
{
    [Obsolete]
    public class ColumnSchemaMap : Dictionary<int, List<InventorySettings.ColumnSchema>>
    {
        public ColumnSchemaMap(int capacity) : base(capacity) { }
    }
    public class SettingsMap : Dictionary<int, InventorySettings>
    {
        public SettingsMap(int capacity) : base(capacity) { }
    }
}

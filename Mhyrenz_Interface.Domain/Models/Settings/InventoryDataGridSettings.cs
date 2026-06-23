using System.Collections.Generic;

namespace Mhyrenz_Interface.Domain.Models.Settings
{
    public class InventoryDataGridSettings : List<InventoryDataGridColumnSetting>
    {
        public InventoryDataGridSettings()
        {
        }

        public InventoryDataGridSettings(
            IEnumerable<InventoryDataGridColumnSetting> collection)
            : base(collection)
        {
        }
    }
}

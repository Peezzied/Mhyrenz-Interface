using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services.Settings
{
    public class InventoryDataGridSettings: List<InventoryDataGridColumnSetting>
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

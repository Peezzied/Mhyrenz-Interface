using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Mhyrenz_Interface.Core
{
    public class InventoryDataGridSettingsProvider
    {
        public Dictionary<int, Dictionary<string, ColumnSetting>> Categories { get; set; }
        public InventoryDataGridSettingsProvider()
        {
            var inventoryDataGridSettings = Properties.Settings.Default.InventoryDataGrid;
            if (string.IsNullOrWhiteSpace(inventoryDataGridSettings))
            {
                Categories = new Dictionary<int, Dictionary<string, ColumnSetting>>();
            }
            else
            {
                Categories = JsonSerializer.Deserialize<Dictionary<int, Dictionary<string, ColumnSetting>>>(inventoryDataGridSettings);
            }
        }

        public void Save()
        {
            Properties.Settings.Default.InventoryDataGrid = JsonSerializer.Serialize(Categories);
            Properties.Settings.Default.Save();
        }

    }

    public class ColumnSetting
    {
        public int DisplayIndex { get; set; } = -1;
        public bool IsVisible { get; set; } = true;
    }
}

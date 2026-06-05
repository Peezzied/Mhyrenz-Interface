using System.Collections.Generic;
using System.Text.Json;

namespace Mhyrenz_Interface.Core
{
    public class InventoryDataGridSettingsProvider
    {
        private Dictionary<int, Dictionary<string, ColumnSetting>> _categories;

        public Dictionary<int, Dictionary<string, ColumnSetting>> Categories
        {
            get => _categories ?? (_categories = Load());
        }

        private Dictionary<int, Dictionary<string, ColumnSetting>> Load()
        {
            var json = Properties.Settings.Default.InventoryDataGrid;
            if (!string.IsNullOrWhiteSpace(json))
            {
                return JsonSerializer.Deserialize<Dictionary<int,
                    Dictionary<string, ColumnSetting>>>(json)
                    ?? new Dictionary<int, Dictionary<string, ColumnSetting>>();
            }
            return new Dictionary<int, Dictionary<string, ColumnSetting>>();
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

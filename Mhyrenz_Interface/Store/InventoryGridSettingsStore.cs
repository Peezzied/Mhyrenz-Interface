using Mhyrenz_Interface.Domain.Models.Settings;
using Newtonsoft.Json;

namespace Mhyrenz_Interface.Store
{
    public static class InventoryDataGridSettingsStore
    {
        private const string SettingsKey = "InventoryDataGrid";

        public static InventoryDataGridSettings Load()
        {
            var json = Properties.Settings.Default[SettingsKey] as string;

            if (string.IsNullOrWhiteSpace(json))
                return new InventoryDataGridSettings();

            try
            {
                var data = JsonConvert.DeserializeObject<InventoryDataGridSettings>(json);
                return data ?? new InventoryDataGridSettings();
            }
            catch
            {
                return new InventoryDataGridSettings();
            }
        }

        public static void Save(InventoryDataGridSettings settings)
        {
            if (settings == null)
                settings = new InventoryDataGridSettings();

            var json = JsonConvert.SerializeObject(settings, Formatting.None);

            Properties.Settings.Default[SettingsKey] = json;
            Properties.Settings.Default.Save();
        }
    }
}

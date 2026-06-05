using System.Collections.Generic;
using Mhyrenz_Interface.Core.Utilities;
using Mhyrenz_Interface.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mhyrenz_Interface.Core
{
    public class InventorySettingsProvider
    {
        private readonly IOptionsMonitor<List<InventorySettings>> _monitor;
        private readonly IServiceScopeFactory _scopeFactory;

        public SettingsMap SettigsMap { get; private set; }
        public ColumnSchemaMap ColumnSchemaMap { get; private set; }

        public InventorySettingsProvider(IOptionsMonitor<List<InventorySettings>> monitor, IOptions<List<InventorySettings>> monitor1)
        {
            _monitor = monitor;
            _monitor.OnChange(_ => Load());
        }

        public void Load()
        {
            var columnMap = new ColumnSchemaMap(_monitor.CurrentValue.Count);
            var settingsMap = new SettingsMap(_monitor.CurrentValue.Count);

            foreach (var setting in _monitor.CurrentValue)
            {
                if (setting.ExtraColumns != null)
                    columnMap[setting.Id] = setting.ExtraColumns;

                settingsMap[setting.Id] = setting;
            }

            ColumnSchemaMap = columnMap;
            SettigsMap = settingsMap;

        }

    }
}

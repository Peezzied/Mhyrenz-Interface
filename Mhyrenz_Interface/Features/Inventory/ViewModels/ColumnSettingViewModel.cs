using System.Linq;
using System.Runtime.CompilerServices;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.Settings;
using Microsoft.Extensions.Options;

namespace Mhyrenz_Interface.Features.Inventory.ViewModels
{
    public class ColumnSettingViewModel : BaseViewModel
    {
        private readonly InventoryDataGridColumnSetting _columnSetting;
        private readonly ConfigManager<InventoryDataGridSettings> _inventoryDataGridSettings;
        private readonly IOptionsMonitor<InventoryDataGridSettings> _inventoryDataGridSettingsProvider;

        public ColumnSettingViewModel(InventoryDataGridColumnSetting columnSetting, ConfigManager<InventoryDataGridSettings> inventoryDataGridSettings, IOptionsMonitor<InventoryDataGridSettings> inventoryDataGridSettingsProvider)
        {
            _columnSetting = columnSetting;
            _inventoryDataGridSettings = inventoryDataGridSettings;
            _inventoryDataGridSettingsProvider = inventoryDataGridSettingsProvider;
        }

        public void Initialize(bool isVisible, int displayIndex, string name, bool hidden, bool isDraggable)
        {
            // Set backing fields directly — no OnPropertyChanged, no Save()
            _isVisible = isVisible;
            _displayIndex = displayIndex;
            Name = name;
            Hidden = hidden;
            IsDraggable = isDraggable;

            // Sync to model without saving
            _columnSetting.IsVisible = isVisible;
            _columnSetting.DisplayIndex = displayIndex;

            OnPropertyChanged(null);

            _isSaveEnabled = true;
        }

        private bool _isSaveEnabled;

        public string Name { get; set; }

        private bool _isVisible;
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                _columnSetting.IsVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public bool IsDraggable { get; internal set; }

        public bool Hidden { get; internal set; }

        private int _displayIndex;
        public int DisplayIndex
        {
            get
            {
                return _displayIndex;
            }
            set
            {
                _displayIndex = value;
                _columnSetting.DisplayIndex = value;
                OnPropertyChanged(nameof(DisplayIndex));
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (_isSaveEnabled)
            {
                var settings = _inventoryDataGridSettingsProvider.CurrentValue
                    .Where(x => x.Header != Name)
                    .ToList();

                settings.Add(_columnSetting);

                _inventoryDataGridSettings.Save(new InventoryDataGridSettings(settings));
            }
        }
    }
}

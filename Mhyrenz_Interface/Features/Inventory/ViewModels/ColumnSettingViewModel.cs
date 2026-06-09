using System;
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

        public void Initialize(bool isVisible, int displayIndex, string name, bool hidden, bool isDraggable, bool placeOrderBound)
        {
            Name = name;
            Hidden = hidden;
            IsDraggable = isDraggable;
            PlaceOrderBound = placeOrderBound;

            // Sync to model without saving
            _columnSetting.IsVisible = isVisible;
            _columnSetting.DisplayIndex = displayIndex;

            OnPropertyChanged(null);

            _isSaveEnabled = true;
        }

        private bool _isSaveEnabled;

        public string Name { get; set; }

        public bool IsVisible
        {
            get => _columnSetting.IsVisible;
            set
            {
                _columnSetting.IsVisible = value;

                OnPropertyChanged(nameof(IsVisible));
                _isSaveEnabled = true;
            }
        }

        public bool IsDraggable { get; internal set; }
        public bool PlaceOrderBound { get; private set; }
        public bool Hidden { get; internal set; }

        public int DisplayIndex
        {
            get => _columnSetting.DisplayIndex;
            set
            {
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

        internal void SuppressSave()
        {
            _isSaveEnabled = false;
        }
    }
}

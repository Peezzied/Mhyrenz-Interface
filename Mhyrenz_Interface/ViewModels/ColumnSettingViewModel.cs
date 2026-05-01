using System.Runtime.CompilerServices;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.ViewModels
{
    public class ColumnSettingViewModel : BaseViewModel
    {
        private readonly ColumnSetting _columnSetting;
        private readonly InventoryDataGridSettingsProvider _inventoryDataGridSettingsProvider;

        public ColumnSettingViewModel(ColumnSetting columnSetting, InventoryDataGridSettingsProvider inventoryDataGridSettingsProvider)
        {
            _columnSetting = columnSetting;
            _inventoryDataGridSettingsProvider = inventoryDataGridSettingsProvider;
        }

        public Category Owner { get; set; }
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
            _inventoryDataGridSettingsProvider.Save();
        }
    }
}

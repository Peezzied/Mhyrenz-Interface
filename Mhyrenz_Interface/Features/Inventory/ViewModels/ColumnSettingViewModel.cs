using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Domain.Models.Settings;

namespace Mhyrenz_Interface.Features.Inventory.ViewModels
{
    public class ColumnSettingViewModel : BaseViewModel
    {

        public ColumnSettingViewModel(InventoryDataGridColumnSetting columnSetting)
        {
            ColumnSetting = columnSetting;
        }

        public void Initialize(bool isVisible, int displayIndex, string name, bool hidden, bool isDraggable, bool placeOrderBound)
        {
            Name = name;
            Hidden = hidden;
            IsDraggable = isDraggable;
            PlaceOrderBound = placeOrderBound;

            ColumnSetting.IsVisible = isVisible;
            ColumnSetting.DisplayIndex = displayIndex;

            OnPropertyChanged(null);
        }

        public InventoryDataGridColumnSetting ColumnSetting { get; }

        public string Name { get; set; }

        public bool IsVisible
        {
            get => ColumnSetting.IsVisible;
            set
            {
                ColumnSetting.IsVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public bool IsDraggable { get; internal set; }
        public bool PlaceOrderBound { get; private set; }
        public bool Hidden { get; internal set; }

        public int DisplayIndex
        {
            get => ColumnSetting.DisplayIndex;
            set
            {
                ColumnSetting.DisplayIndex = value;
                OnPropertyChanged(nameof(DisplayIndex));
            }
        }
    }
}

namespace Mhyrenz_Interface.Domain.Services.Settings
{
    public class InventoryDataGridColumnSetting
    {
        public InventoryDataGridColumnSetting()
        {
            
        }

        public InventoryDataGridColumnSetting(InventoryDataGridColumnSetting @this)
        {
            Header = @this.Header;
            DisplayIndex = @this.DisplayIndex;
            IsVisible = @this.IsVisible;
        }

        public string Header { get; set; }
        public int DisplayIndex { get; set; } = -1;
        public bool PharmaColumn { get; set; } = false;
        public bool IsVisible { get; set; } = true;
    }
}

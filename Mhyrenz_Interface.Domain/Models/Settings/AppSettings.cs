namespace Mhyrenz_Interface.Domain.Models.Settings
{
    public class AppSettings
    {
        public string ExportTemplate { get; set; }
        public string BarcodePort { get; set; } = "COM2";
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Mhyrenz_Interface.Domain.Services.BarcodeCacheService
{
    public interface IBarcodeImageCache
    {
        BitmapImage GetOrCreate(string code);
        Task Preload(IEnumerable<string> codes);
    }
}
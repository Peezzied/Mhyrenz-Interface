using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Mhyrenz_Interface.Domain.Services.BarcodeCacheService
{
    public interface IBarcodeImageCache
    {
        BitmapImage GetOrCreate(string code);
        void Preload(IEnumerable<string> codes);
    }
}
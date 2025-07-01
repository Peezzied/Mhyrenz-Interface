using System.IO;

namespace Mhyrenz_Interface.Domain.Services.BarcodeCacheService
{
    public class CachePath : ICachePath
    {
        public string Dir { get; private set; }

        public CachePath()
        {
            string cacheDir = Path.Combine(Path.GetTempPath(), "Mhyrenz Interface Barcodes");
            if (!File.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);
            
            Dir = cacheDir;
        }
    }
}
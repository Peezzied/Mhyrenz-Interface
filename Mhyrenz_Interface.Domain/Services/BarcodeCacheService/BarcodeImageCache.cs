using Mhyrenz_Interface.Domain.Exceptions;
using Mhyrenz_Interface.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using ZXing;

namespace Mhyrenz_Interface.Domain.Services.BarcodeCacheService
{
    public class BarcodeImageCache : IBarcodeImageCache
    {
        private readonly ConcurrentDictionary<string, BitmapImage> _memoryCache = new ConcurrentDictionary<string, BitmapImage>();
        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();
        private readonly string _cacheDir;

        public Task Preload(IEnumerable<string> codes)
        {
            return Task.Run(() =>
            {
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                Parallel.ForEach(codes, parallelOptions, code =>
                {
                    if (string.IsNullOrWhiteSpace(code))
                        return;

                    var image = GetOrCreate(code);
                    if (image != null)
                        _memoryCache[code] = image;
                });

                FlushUncached();
            });
        }


        public void FlushUncached()
        {
            var cachedFilePaths = new HashSet<string>(
                _memoryCache.Keys.Select(code => GetCacheFilePath(code)),
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var file in Directory.EnumerateFiles(_cacheDir, "*.png"))
            {
                if (!cachedFilePaths.Contains(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete '{file}': {ex.Message}");
                    }
                }
            }
        }


        public BarcodeImageCache(ICachePath cachePath)
        {
            _cacheDir = cachePath.Dir;
        }

        public static async Task<BarcodeImageCache> LoadBarcodeImageCache(IEnumerable<Product> products, ICachePath cachePath)
        {
            var preCache = products
                    .Where(p => !string.IsNullOrEmpty(p.Barcode))
                    .Select(p => p.Barcode);

            var instance = new BarcodeImageCache(cachePath);
            await instance.Preload(preCache);

            return instance;
        }

        public BitmapImage GetOrCreate(string code)
        {
            if (_memoryCache.TryGetValue(code, out var cached))
                return cached;

            var codeLock = _locks.GetOrAdd(code, _ => new object());

            lock (codeLock)
            {
                if (_memoryCache.TryGetValue(code, out cached))
                    return cached;

                string cachePath = GetCacheFilePath(code);
                BitmapImage bitmap;

                if (File.Exists(cachePath))
                {
                    bitmap = LoadBitmapImageFromCache(cachePath);
                }
                else
                {
                    try
                    {
                        bitmap = GenerateBarcodeImage(code);
                        CacheBitmapImage(bitmap, cachePath);
                    }
                    catch
                    {
                        return null;
                    }
                }

                _memoryCache[code] = bitmap;
                return bitmap;
            }
        }


        private static void CacheBitmapImage(BitmapImage bitmap, string cachePath)
        {
            using (var stream = new FileStream(cachePath, FileMode.Create, FileAccess.Write))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
            }
        }

        private string GetCacheFilePath(string image)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(image));
                string hashStr = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return Path.Combine(_cacheDir, hashStr + ".png");
            }
        }

        private BitmapImage LoadBitmapImageFromCache(string path)
        {
            var image = new BitmapImage();

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
            }

            return image;
        }

        private BitmapImage GenerateBarcodeImage(string code)
        {
            try
            {
                var writer = new BarcodeWriter
                {
                    Format = GuessFormat(code),
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Width = 150,
                        Height = 30,
                        Margin = 4
                    }
                };

                var bitmap = writer.Write(code);
                return ConvertToBitmapImage(bitmap);
            }
            catch
            {
                throw new InvalidBarcodeEntryException(code);
            }
        }

        private BitmapImage ConvertToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = memory;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        private BarcodeFormat GuessFormat(string input)
        {
            return Regex.IsMatch(input, @"^\d{12}$") ? BarcodeFormat.UPC_A : BarcodeFormat.EAN_13;
        }
    }

}

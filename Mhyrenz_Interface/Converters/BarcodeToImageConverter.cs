using System;
using System.Globalization;
using System.Windows.Data;
using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.Converters
{
    public class BarcodeToImageConverter : IValueConverter
    {
        private readonly IBarcodeImageCache _cache;

        public BarcodeToImageConverter(IBarcodeImageCache cache)
        {
            _cache = cache;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string code) || string.IsNullOrWhiteSpace(code))
                return null;

            var cache = _cache.GetOrCreate(code);
            if (!(cache is null))
                return cache;
            else
                return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

}

using System;
using System.Globalization;
using System.Windows.Data;
using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.Converters
{
    public class BarcodeToImageConverter : IValueConverter
    {
        public static IBarcodeImageCache Cache => App.ServiceProvider?.GetService<IBarcodeImageCache>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string code) || string.IsNullOrWhiteSpace(code))
                return null;

            var cache = Cache.GetOrCreate(code);
            if (!(cache is null))
                return cache;
            else
                return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

}

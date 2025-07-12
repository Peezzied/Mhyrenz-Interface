using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using ZXing;

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
                return "Invalid";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

}

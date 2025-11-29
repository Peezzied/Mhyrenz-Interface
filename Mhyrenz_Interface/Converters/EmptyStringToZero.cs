using System;
using System.Globalization;
using System.Windows.Data;

namespace Mhyrenz_Interface.Converters
{
    public class EmptyStringToZero : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value?.ToString() ?? default;
            return string.IsNullOrEmpty(str) ? 0 : value;
        }
    }
}

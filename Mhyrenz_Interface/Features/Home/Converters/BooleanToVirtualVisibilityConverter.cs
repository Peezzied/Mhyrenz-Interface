using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mhyrenz_Interface.Features.Home.Converters
{
    internal class BooleanToVirtualVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isVisible = value is bool b && b;

            return isVisible ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility != Visibility.Visible;

            return false;
        }
    }
}

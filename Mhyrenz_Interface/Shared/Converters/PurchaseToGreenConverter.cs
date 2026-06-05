using System;
using System.Globalization;
using System.Windows.Data;

namespace Mhyrenz_Interface.Shared.Converters
{
    public class PurchaseToGreenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int intValue && intValue > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

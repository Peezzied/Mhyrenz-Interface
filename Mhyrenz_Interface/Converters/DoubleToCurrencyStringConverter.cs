using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Mhyrenz_Interface.Converters
{
    public class DoubleToCurrencyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
                return d.ToString("C", culture ?? new CultureInfo("en-PH"));
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value?.ToString(), NumberStyles.Currency, culture ?? new CultureInfo("en-PH"), out double result))
                return result;
            return null;
        }
    }

}

using System;
using System.Globalization;
using System.Windows.Data;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Shared.Converters
{
    public class DiscountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Discount discount)
            {
                return discount != Discount.None;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

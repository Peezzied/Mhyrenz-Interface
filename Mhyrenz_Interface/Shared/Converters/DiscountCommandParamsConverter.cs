using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Shared.Converters
{
    public class DiscountCommandParams
    {
        public Discount Discount { get; set; }
        public IList Transactions { get; set; }
    }

    public class DiscountCommandParamsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return new DiscountCommandParams
            {
                Discount = (Discount)values[0],
                Transactions = (IList)values[1]
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

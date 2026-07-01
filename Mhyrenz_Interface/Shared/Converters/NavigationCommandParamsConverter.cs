using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.Controls;
using MenuItem = Mhyrenz_Interface.Shared.Controls.MenuItem;

namespace Mhyrenz_Interface.Shared.Converters
{
    [Obsolete]
    public class NavigationCommandParams
    {
        public HamburgerMenu Menu { get; set; }
        public ObservableCollection<MenuItem> MenuItem { get; set; }
    }
    [Obsolete]
    public class NavigationCommandParamsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return new NavigationCommandParams
            {
                Menu = values[0] as HamburgerMenu,
                MenuItem = values[1] as ObservableCollection<MenuItem>
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

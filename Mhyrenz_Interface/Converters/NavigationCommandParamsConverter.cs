using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuItem = Mhyrenz_Interface.Controls.MenuItem;
using System.Windows.Data;

namespace Mhyrenz_Interface.Converters
{
    public class NavigationCommandParams
    {
        public HamburgerMenu Menu { get; set; }
        public ObservableCollection<MenuItem> MenuItem { get; set; }
    }
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

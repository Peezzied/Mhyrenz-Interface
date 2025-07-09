using HandyControl.Controls;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mhyrenz_Interface.Controls
{
    public class NumericUpDownEx
    {
        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.RegisterAttached(
            "StringFormat", typeof(string), typeof(NumericUpDownEx), new PropertyMetadata("G"));

        public static void SetStringFormat(DependencyObject element, string value)
            => element.SetValue(StringFormatProperty, value);

        public static string GetStringFormat(DependencyObject element)
            => (string)element.GetValue(StringFormatProperty);

        public static readonly DependencyProperty CultureInfoProperty = DependencyProperty.RegisterAttached(
            "CultureInfo", typeof(string), typeof(NumericUpDownEx), new PropertyMetadata(null));

        public static void SetCulture(DependencyObject element, string value)
            => element.SetValue(CultureInfoProperty, value);

        public static string GetCulture(DependencyObject element)
            => (string)element.GetValue(CultureInfoProperty);
    }


    /// <summary>
    /// Interaction logic for AddProductDrawer.xaml
    /// </summary>
    public partial class AddProductDrawer : UserControl
    {
        public AddProductDrawer()
        {
            InitializeComponent();

            Loaded += OnLoad;
            Unloaded += OnUnload;

        }

        private void OnUnload(object sender, RoutedEventArgs e)
        {
            DataContext.CastTo<AddProductViewModel>().DrawerClose -= DrawerClose;
        }

        private void DrawerClose()
        {
            ClearNestedNumericValidation(RetailPriceNumericUpDown);
            ClearNestedNumericValidation(PrincipalStockNumericUpDown);
        }

        private void ClearNestedNumericValidation(DependencyObject container)
        {
            var innerNumeric = TreeHelper.FindChild<MahApps.Metro.Controls.NumericUpDown>(container);
            if (innerNumeric != null)
            {
                var binding = innerNumeric.GetBindingExpression(MahApps.Metro.Controls.NumericUpDown.ValueProperty);
                if (binding != null)
                {
                    Validation.ClearInvalid(binding);
                }
            }
        }


        private void OnLoad(object sender, RoutedEventArgs e)
        {
            DataContext.CastTo<AddProductViewModel>().DrawerClose += DrawerClose;
        }
    }
}

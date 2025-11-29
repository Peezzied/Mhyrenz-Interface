using System.Windows;
using System.Windows.Controls;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.ViewModels;

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

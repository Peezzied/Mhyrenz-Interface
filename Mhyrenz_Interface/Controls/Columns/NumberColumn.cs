using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MahApps.Metro.Controls;
using Style = System.Windows.Style;

namespace Mhyrenz_Interface.Controls.Columns
{
    public class NumberColumn : DataGridNumericUpDownColumn
    {
        public Style NumberControlStyle
        {
            get { return (Style)GetValue(NumberControlStyleProperty); }
            set { SetValue(NumberControlStyleProperty, value); }
        }

        public static readonly DependencyProperty NumberControlStyleProperty =
            DependencyProperty.Register(nameof(NumberControlStyle), typeof(Style), typeof(NumberColumn), new PropertyMetadata(null));


        private BindingBase _binding;
        public virtual BindingBase DisplayBinding
        {
            get
            {
                return _binding;
            }
            set
            {
                if (_binding != value)
                {
                    BindingBase binding = _binding;
                    _binding = value;
                    CoerceValue(DataGridColumn.IsReadOnlyProperty);
                    CoerceValue(DataGridColumn.SortMemberPathProperty);
                    OnBindingChanged(binding, _binding);
                }
            }
        }

        public NumberColumn()
        {
            TextAlignment = TextAlignment.Center;
            Culture = new CultureInfo("en-Ph");
        }


        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            var numericUpDown = base.GenerateEditingElement(cell, dataItem) as NumericUpDown;
            numericUpDown.Style = NumberControlStyle ?? default;
            return CellAdornerHelper.ApplyAdorner(numericUpDown, NumericUpDown.ValueProperty, 
                TreeHelper.TryFindParent<DataGrid>(cell).DataContext);
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var textBlock = new TextBlock
            {
                TextAlignment = TextAlignment.Center,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = Application.Current.TryFindResource("MahApps.Styles.TextBlock.DataGrid") as Style ?? default
            };

            textBlock.SetBinding(TextBlock.TextProperty, Binding ?? DisplayBinding);

            return textBlock;
        }
    }
}

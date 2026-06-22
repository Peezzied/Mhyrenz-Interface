using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HandyControl.Controls;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Shared.Adorners;
using MessageBox = HandyControl.Controls.MessageBox;
using NumericUpDown = MahApps.Metro.Controls.NumericUpDown;
using Style = System.Windows.Style;

namespace Mhyrenz_Interface.Shared.Columns
{
    public class NumberColumn : DataGridNumericUpDownColumn
    {
        public Style Style
        {
            get { return (Style)GetValue(StyleProperty); }
            set { SetValue(StyleProperty, value); }
        }

        public static readonly DependencyProperty StyleProperty =
            DependencyProperty.Register(nameof(Style), typeof(Style), typeof(NumberColumn), new PropertyMetadata(null));


        private BindingBase _displayBinding;
        public BindingBase DisplayBinding
        {
            get
            {
                return _displayBinding;
            }
            set
            {
                if (_displayBinding != value)
                {
                    BindingBase binding = _displayBinding;
                    _displayBinding = value;
                    CoerceValue(DataGridColumn.IsReadOnlyProperty);
                    CoerceValue(DataGridColumn.SortMemberPathProperty);
                    OnBindingChanged(binding, _displayBinding);
                }
            }
        }

        public NumberColumn()
        {
            TextAlignment = TextAlignment.Center;
        }


        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            if (!cell.IsEditable(dataItem))
            {
                cell.IsEditing = false;
                return base.GenerateElement(cell, dataItem);
            }

            var numericUpDown = base.GenerateEditingElement(cell, dataItem) as NumericUpDown;
            numericUpDown.Style = Style ?? default;
            return CellAdornerHelper.ApplyAdorner(numericUpDown, NumericUpDown.ValueProperty,
                TreeHelper.TryFindParent<DataGrid>(cell).DataContext);
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var textBlock = new TextBlock
            {
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = Application.Current.TryFindResource("MahApps.Styles.TextBlock.DataGrid") as Style ?? default
            };

            textBlock.TextAlignment = TextAlignment;

            textBlock.SetBinding(TextBlock.TextProperty, Binding ?? DisplayBinding);

            return textBlock;
        }
    }
}

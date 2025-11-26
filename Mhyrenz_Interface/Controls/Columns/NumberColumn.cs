using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MahApps.Metro.Controls;
using Style = System.Windows.Style;

namespace Mhyrenz_Interface.Controls.Columns
{
    public class NumberColumn : BaseTemplateColumn
    {
        public string DisplayPath
        {
            get { return (string)GetValue(DisplayPathProperty); }
            set { SetValue(DisplayPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayPathProperty =
            DependencyProperty.Register(nameof(DisplayPath), typeof(string), typeof(NumberColumn), new PropertyMetadata(null));


        public string MinimumPath
        {
            get { return (string)GetValue(MinimumPathProperty); }
            set { SetValue(MinimumPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinimumPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumPathProperty =
            DependencyProperty.Register(nameof(MinimumPath), typeof(string), typeof(NumberColumn), new PropertyMetadata(null));


        public Style NumberControlStyle
        {
            get { return (Style)GetValue(NumberControlStyleProperty); }
            set { SetValue(NumberControlStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NumberControlStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberControlStyleProperty =
            DependencyProperty.Register(nameof(NumberControlStyle), typeof(Style), typeof(NumberColumn), new PropertyMetadata(null));


        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumberColumn), new FrameworkPropertyMetadata(0.00));


        public ButtonsAlignment ButtonsAlignment
        {
            get { return (ButtonsAlignment)GetValue(ButtonsAlignmentProperty); }
            set { SetValue(ButtonsAlignmentProperty, value); }
        }

        public static readonly DependencyProperty ButtonsAlignmentProperty =
            DependencyProperty.Register(
                nameof(ButtonsAlignment),
                typeof(ButtonsAlignment),
                typeof(NumberColumn),
                new FrameworkPropertyMetadata(ButtonsAlignment.Right, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));


        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StringFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(NumberColumn), new PropertyMetadata(string.Empty));

        protected override (FrameworkElement Element, DependencyProperty Property) EditingElement()
        {
            var numeric = new NumericUpDown
            {
                NumericInputMode = NumericInput.Numbers,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                StringFormat = StringFormat ?? default,
                Style = NumberControlStyle ?? default,
                Culture = new CultureInfo("en-PH")
            };

            if (ValuePath != null)
                numeric.SetBinding(NumericUpDown.ValueProperty, new Binding(ValuePath) { UpdateSourceTrigger = UpdateSourceTrigger.Explicit });

            if (MinimumPath != null)
                numeric.SetBinding(NumericUpDown.MinimumProperty, new Binding(MinimumPath));
            else
                numeric.SetValue(NumericUpDown.MinimumProperty, Minimum);

            numeric.SetBinding(NumericUpDown.ButtonsAlignmentProperty, new Binding(nameof(ButtonsAlignment)) { Source = this });
            numeric.SetBinding(NumericUpDown.StringFormatProperty, new Binding(nameof(StringFormat)) { Source = this });

            return (numeric, NumericUpDown.ValueProperty);
        }

        protected override FrameworkElement Element()
        {
            var textBlock = new TextBlock
            {
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = Application.Current.TryFindResource("MahApps.Styles.TextBlock.DataGrid") as Style ?? default
            };

            if (ValuePath != null || DisplayPath != null)
            {
                textBlock.SetBinding(TextBlock.TextProperty, new Binding(ValuePath ?? DisplayPath)
                {
                    StringFormat = StringFormat ?? default,
                    ConverterCulture = new CultureInfo("en-PH"), // REFACTOR THIS TO BE SOURCED FROM GLOBAL CONFIG
                });
            }

            return textBlock;
        }

    }
}

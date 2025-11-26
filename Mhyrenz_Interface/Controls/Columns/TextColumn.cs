using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mhyrenz_Interface.Controls.Columns
{
    public class TextColumn : BaseTemplateColumn
    {
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(TextColumn), new PropertyMetadata(TextAlignment.Left));


        protected override (FrameworkElement Element, DependencyProperty Property) EditingElement()
        {
            var textBox = new TextBox
            {
                Margin = new Thickness(2),
                Style = Application.Current.TryFindResource("MahApps.Styles.TextBox.DataGrid.Editing") as Style ?? default
            };

            if (ValuePath != null)
                textBox.SetBinding(TextBox.TextProperty, new Binding(ValuePath) { UpdateSourceTrigger = UpdateSourceTrigger.Explicit });

            return (textBox, TextBox.TextProperty);
        }
        protected override FrameworkElement Element()
        {
            var textBlock = new MaxLinesTextBlock
            {
                TextAlignment = TextAlignment,
                LineHeight = 16,
                MaxLines = 3,
                Padding = new Thickness(4),
                Width = Double.NaN
            };

            if (ValuePath != null)
                textBlock.SetBinding(TextBlock.TextProperty, new Binding(ValuePath));

            return textBlock;
        }

    }
}

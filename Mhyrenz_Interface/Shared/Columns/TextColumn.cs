using System;
using System.Windows;
using System.Windows.Controls;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Shared.Controls;

namespace Mhyrenz_Interface.Shared.Columns
{
    public class TextColumn : DataGridTextColumn
    {
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(TextColumn), new PropertyMetadata(TextAlignment.Left));

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var textBlock = new MaxLinesTextBlock
            {
                TextAlignment = TextAlignment,
                LineHeight = 16,
                MaxLines = 3,
                Padding = new Thickness(4),
                Width = Double.NaN
            };
            textBlock.SetBinding(TextBlock.TextProperty, Binding);
            return textBlock;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            if (!cell.IsEditable(((ProductDataViewModel)dataItem).Item))
            {
                cell.IsEditing = false;
                return base.GenerateElement(cell, dataItem);
            }

            return base.GenerateEditingElement(cell, dataItem);
        }
    }
}

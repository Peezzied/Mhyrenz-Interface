using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mhyrenz_Interface.Controls
{
    public class MaxLinesTextBlock : TextBlock
    {
        public static readonly DependencyProperty MaxLinesProperty =
            DependencyProperty.Register(
                nameof(MaxLines),
                typeof(int),
                typeof(MaxLinesTextBlock),
                new PropertyMetadata(0, OnMaxLinesChanged));

        public int MaxLines
        {
            get => (int)GetValue(MaxLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }

        private static void OnMaxLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MaxLinesTextBlock textBlock)
            {
                textBlock.UpdateMaxHeight();
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            UpdateMaxHeight();
        }

        private void UpdateMaxHeight()
        {
            if (MaxLines > 0 && LineHeight > 0)
            {
                MaxHeight = MaxLines * LineHeight;
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                TextTrimming = TextTrimming.WordEllipsis;
                TextWrapping = TextWrapping.Wrap;
            }
        }
    }
}

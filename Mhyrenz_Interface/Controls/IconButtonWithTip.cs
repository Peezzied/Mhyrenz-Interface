using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mhyrenz_Interface.Controls
{
    public class IconButtonWithTip : Button
    {
        public static readonly DependencyProperty TipTextColorProperty =
    DependencyProperty.Register(
        nameof(TipTextColor),
        typeof(Brush),
        typeof(IconButtonWithTip),
        new PropertyMetadata(null));

        public Brush TipTextColor
        {
            get => (Brush)GetValue(TipTextColorProperty);
            set => SetValue(TipTextColorProperty, value);
        }


        public static readonly DependencyProperty TiptextProperty =
            DependencyProperty.Register(nameof(Tiptext), typeof(string), typeof(IconButtonWithTip), new PropertyMetadata(null));

        public string Tiptext
        {
            get => (string)GetValue(TiptextProperty);
            set => SetValue(TiptextProperty, value);
        }

        public static readonly DependencyProperty IconContentProperty =
            DependencyProperty.Register(nameof(IconContent), typeof(UIElement), typeof(IconButtonWithTip), new PropertyMetadata(null));

        public UIElement IconContent
        {
            get => (UIElement)GetValue(IconContentProperty);
            set => SetValue(IconContentProperty, value);
        }
    }

}

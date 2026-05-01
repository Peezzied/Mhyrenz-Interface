using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Mhyrenz_Interface.Controls
{
    public class CategoryTag : Border
    {
        private readonly TextBlock _textBlock;

        public CategoryTag()
        {
            // Create inner TextBlock
            _textBlock = new TextBlock
            {
                FontFamily = new FontFamily("Arial"),
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Set default appearance of Border
            Height = 20;
            Padding = new Thickness(10, 0, 10, 0);
            CornerRadius = new CornerRadius(10); // rounded corners

            Child = _textBlock;

            // Bind TextBlock.Text to CategoryName
            _textBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(CategoryName)) { Source = this });

            // Bind Border.Background to CategoryColor
            SetBinding(BackgroundProperty, new Binding(nameof(CategoryColor)) { Source = this });
        }

        // ─── Dependency Properties ────────────────────────────────

        public string CategoryName
        {
            get => (string)GetValue(CategoryNameProperty);
            set => SetValue(CategoryNameProperty, value);
        }

        public static readonly DependencyProperty CategoryNameProperty =
            DependencyProperty.Register(
                nameof(CategoryName),
                typeof(string),
                typeof(CategoryTag),
                new PropertyMetadata(default(string)));

        public Brush CategoryColor
        {
            get => (Brush)GetValue(CategoryColorProperty);
            set => SetValue(CategoryColorProperty, value);
        }

        public static readonly DependencyProperty CategoryColorProperty =
            DependencyProperty.Register(
                nameof(CategoryColor),
                typeof(Brush),
                typeof(CategoryTag),
                new PropertyMetadata(default(Brush)));
    }
}

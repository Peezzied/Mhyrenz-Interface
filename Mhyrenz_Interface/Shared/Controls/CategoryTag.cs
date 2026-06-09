using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Mhyrenz_Interface.Shared.Controls
{
    public class CategoryTag : Border
    {
        private readonly TextBlock _textBlock;

        public CategoryTag()
        {
            _textBlock = new TextBlock
            {
                FontFamily = new FontFamily("Arial"),
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Height = 20;
            Padding = new Thickness(10, 0, 10, 0);
            CornerRadius = new CornerRadius(10);

            Child = _textBlock;

            _textBlock.SetBinding(TextBlock.TextProperty,
                new Binding(nameof(CategoryName)) { Source = this });

            _textBlock.SetBinding(TextBlock.FontSizeProperty,
                new Binding(nameof(FontSize)) { Source = this});

            SetBinding(BackgroundProperty,
                new Binding(nameof(CategoryColor)) { Source = this });

            IsEnabledChanged += CategoryTag_IsEnabledChanged;

            UpdateDisabledVisualState();
        }

        private void CategoryTag_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateDisabledVisualState();
        }

        private void UpdateDisabledVisualState()
        {
            if (!IsEnabled)
            {
                _textBlock.FontStyle = FontStyles.Italic;
                Background = TryFindResource("MahApps.Brushes.Badged.Foreground.Disabled") as Brush
                             ?? Brushes.Gray;
            }
            else
            {
                _textBlock.FontStyle = FontStyles.Normal;

                BindingOperations.SetBinding(
                    this,
                    BackgroundProperty,
                    new Binding(nameof(CategoryColor)) { Source = this });
            }
        }

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


        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(nameof(FontSize),
                typeof(double),
                typeof(CategoryTag),
                new PropertyMetadata(12d));

    }
}

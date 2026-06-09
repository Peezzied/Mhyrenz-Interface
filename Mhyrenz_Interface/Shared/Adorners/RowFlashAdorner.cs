using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Mhyrenz_Interface.Shared.Adorners
{
    public class RowFlashAdorner : Adorner
    {
        public static readonly DependencyProperty OverlayBrushProperty =
        DependencyProperty.Register(
            nameof(OverlayBrush),
            typeof(Brush),
            typeof(RowFlashAdorner),
            new FrameworkPropertyMetadata(
                Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public RowFlashAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
        }

        public Brush OverlayBrush
        {
            get => (Brush)GetValue(OverlayBrushProperty);
            set => SetValue(OverlayBrushProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (!(AdornedElement is FrameworkElement row))
                return;

            drawingContext.DrawRectangle(
                OverlayBrush,
                null,
                new Rect(0, 0, row.ActualWidth, row.ActualHeight));
        }
    }
}

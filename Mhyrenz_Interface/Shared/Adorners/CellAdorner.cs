using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace Mhyrenz_Interface.Shared.Adorners
{

    [ContentProperty(nameof(Adorned))]
    public class CellAdorner : ContentControl
    {
        class ContentAdorner : Adorner
        {
            private readonly VisualCollection _visuals;
            private readonly FrameworkElement _element;

            public ContentAdorner(UIElement adornedElement, FrameworkElement visual)
                : base(adornedElement)
            {
                //IsHitTestVisible = false;
                //Focusable = false;

                _element = visual;
                _visuals = new VisualCollection(this)
                {
                    visual
                };
            }

            protected override int VisualChildrenCount => _visuals.Count;
            protected override Visual GetVisualChild(int index) => _visuals[index];

            protected override Size MeasureOverride(Size constraint)
            {
                var adornedSize = AdornedElement.RenderSize;

                _element.Measure(constraint);

                var desired = _element.DesiredSize;

                return new Size(
                    Math.Max(adornedSize.Width, desired.Width) - 1, adornedSize.Height - 1);
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                _element.Arrange(new Rect(finalSize));
                return finalSize;
            }
        }

        private DataGrid dataGrid;
        private ContentAdorner _adorner;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Loaded += CellAdorner_Loaded;
        }

        public FrameworkElement Adorned
        {
            get { return (FrameworkElement)GetValue(AdornedProperty); }
            set { SetValue(AdornedProperty, value); }
        }

        public static readonly DependencyProperty AdornedProperty =
            DependencyProperty.Register(nameof(Adorned), typeof(FrameworkElement), typeof(CellAdorner), new PropertyMetadata(null));

        private void CellAdorner_Loaded(object sender, RoutedEventArgs e)
        {
            var cell = TreeHelper.TryFindParent<DataGridCell>(this);

            var content = Adorned as FrameworkElement;

            content.DataContext = cell.DataContext;
            content.Unloaded += Content_LostFocus;

            _adorner = new ContentAdorner(cell, content);
            AdornerLayer.GetAdornerLayer(cell).Add(_adorner);

            dataGrid = TreeHelper.TryFindParent<DataGrid>(cell);
            dataGrid.CellEditEnding += Content_CellEditEnding;
        }

        private void Content_LostFocus(object sender, RoutedEventArgs e)
        {
            dataGrid.CommitEdit();
        }

        private void Content_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(_adorner.AdornedElement);
            layer?.Remove(_adorner);
            _adorner = null;

            dataGrid.CellEditEnding -= Content_CellEditEnding;
        }
    }
}

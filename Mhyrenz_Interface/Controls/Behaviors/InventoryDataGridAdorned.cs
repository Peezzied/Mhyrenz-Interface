using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Controls.Behaviors
{

    [ContentProperty(nameof(Content))]
    public class InventoryDataGridAdorned : Behavior<Grid>
    {
        class ContentAdorner : Adorner
        {
            private readonly VisualCollection _visuals;
            private readonly FrameworkElement _element;

            public ContentAdorner(UIElement adornedElement, FrameworkElement visual)
                : base(adornedElement)
            {
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

        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(FrameworkElement), typeof(InventoryDataGridAdorned), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            var cell = TreeHelper.TryFindParent<DataGridCell>(AssociatedObject);

            Content.DataContext = cell.DataContext;

            _adorner = new ContentAdorner(cell, Content);
            AdornerLayer.GetAdornerLayer(cell).Add(_adorner);

            dataGrid = TreeHelper.TryFindParent<DataGrid>(cell);
            dataGrid.CellEditEnding += InventoryDataGridAdorned_CellEditEnding;
        }

        private void InventoryDataGridAdorned_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(_adorner.AdornedElement);
            layer?.Remove(_adorner);
            _adorner = null;

            dataGrid.CellEditEnding -= InventoryDataGridAdorned_CellEditEnding;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}

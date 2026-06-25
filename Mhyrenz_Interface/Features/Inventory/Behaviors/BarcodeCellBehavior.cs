using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Microsoft.Xaml.Behaviors;
using TextBox = System.Windows.Controls.TextBox;

namespace Mhyrenz_Interface.Features.Inventory.Behaviors
{
    public class BarcodeCellBehavior : Behavior<TextBox>
    {
        public class BarcodeAdorner : Adorner
        {
            private readonly FrameworkElement _child;

            public BarcodeAdorner(UIElement adornedElement, FrameworkElement child) : base(adornedElement)
            {
                _child = child;
                AddVisualChild(_child);

                ClipToBounds = false;
                IsHitTestVisible = false;
            }

            protected override int VisualChildrenCount => 1;

            protected override Visual GetVisualChild(int index) => _child;

            protected override Size MeasureOverride(Size constraint)
            {
                var adornedSize = AdornedElement.RenderSize;

                _child.Measure(constraint);

                var desired = _child.DesiredSize;

                return new Size(
                    Math.Max(adornedSize.Width, desired.Width), adornedSize.Height);
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                _child.Arrange(new Rect(finalSize));
                return finalSize;
            }
        }


        public DataTemplate Popup
        {
            get { return (DataTemplate)GetValue(PopupProperty); }
            set { SetValue(PopupProperty, value); }
        }

        public static readonly DependencyProperty PopupProperty =
            DependencyProperty.Register(nameof(Popup), typeof(DataTemplate), typeof(BarcodeCellBehavior), new PropertyMetadata(default));

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void BarcodeCellBehavior_BarcodeReceived()
        {
            //var dataGrid = TreeHelper.TryFindParent<DataGrid>(AssociatedObject);
            //App.Current.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    dataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            //}), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private void AssociatedObject_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            App.ShellViewModel.OpenMainBarcodeReceiver();
            AssociatedObject.DataContext.CastTo<IBarcodeBound>().UnloadBarcodeReceiver();

            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            App.ShellViewModel.SuspendMainBarcodeReceiver();

            var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);

            if (layer != null)
            {
                var adorner = new BarcodeAdorner(AssociatedObject, (FrameworkElement)Popup.LoadContent());
                layer.Add(adorner);
            }

            AssociatedObject.DataContext.CastTo<IBarcodeBound>().BarcodeReceived += BarcodeCellBehavior_BarcodeReceived;
            AssociatedObject.DataContext.CastTo<IBarcodeBound>().LoadBarcodeReceiver();
        }
    }
}

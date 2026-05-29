using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GongSolutions.Wpf.DragDrop.Utilities;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Commands;
using ZXing.PDF417.Internal;

namespace Mhyrenz_Interface.Controls.Attached
{
    public interface IFlashRequestable
    {
        event EventHandler<RowFlashRequestedEventArgs> FlashRequested;
    }

    public class RowFlashRequestedEventArgs : EventArgs
    {
        public RowFlashRequestedEventArgs(SaleBoundPurchaseCommand.DTO.Type method)
        {
            Method = method;
            Completion = new TaskCompletionSource<bool>();
        }

        public SaleBoundPurchaseCommand.DTO.Type Method { get; private set; }
        public TaskCompletionSource<bool> Completion { get; }
    }

    public static class DataGridFlashBehavior
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached(
                "Enabled",
                typeof(bool),
                typeof(DataGridFlashBehavior),
                new PropertyMetadata(false, OnEnabledChanged));

        public static void SetEnabled(DependencyObject element, bool value)
            => element.SetValue(EnabledProperty, value);

        public static bool GetEnabled(DependencyObject element)
            => (bool)element.GetValue(EnabledProperty);

        public static readonly DependencyProperty FlashBrushProperty =
            DependencyProperty.RegisterAttached(
                "FlashBrush",
                typeof(Brush),
                typeof(DataGridFlashBehavior),
                new PropertyMetadata(Brushes.Transparent));

        public static void SetFlashBrush(DependencyObject element, Brush value)
            => element.SetValue(FlashBrushProperty, value);

        public static Brush GetFlashBrush(DependencyObject element)
            => (Brush)element.GetValue(FlashBrushProperty);

        private static readonly DependencyProperty CollectionChangedHandlerProperty =
            DependencyProperty.RegisterAttached(
                "CollectionChangedHandler",
                typeof(NotifyCollectionChangedEventHandler),
                typeof(DataGridFlashBehavior));

        private static readonly DependencyProperty FlashHandlerProperty =
            DependencyProperty.RegisterAttached(
                "FlashHandler",
                typeof(EventHandler<RowFlashRequestedEventArgs>),
                typeof(DataGridFlashBehavior));

        private static void OnEnabledChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DataGrid grid))
                return;

            if ((bool)e.NewValue)
            {
                grid.Loaded += Grid_Loaded;
                grid.Unloaded += Grid_Unloaded;

                Subscribe(grid);
            }
            else
            {
                grid.Loaded -= Grid_Loaded;
                grid.Unloaded -= Grid_Unloaded;

                Unsubscribe(grid);
            }
        }

        private static void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Subscribe((DataGrid)sender);
        }

        private static void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            Unsubscribe((DataGrid)sender);
        }

        private static void Subscribe(DataGrid grid)
        {
            Unsubscribe(grid);

            if (!(grid.ItemsSource is IEnumerable items))
                return;

            EventHandler<RowFlashRequestedEventArgs> flashHandler = async (sender, e) =>
            {
                if (sender != null)
                    await ScrollAndFlashAsync(grid, sender, e);
            };

            grid.SetValue(FlashHandlerProperty, flashHandler);

            foreach (var item in items)
            {
                if (item is IFlashRequestable vm)
                    vm.FlashRequested += flashHandler;
            }

            if (grid.ItemsSource is INotifyCollectionChanged observable)
            {
                NotifyCollectionChangedEventHandler collectionHandler = (sender, e) =>
                {
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            if (item is IFlashRequestable vm)
                                vm.FlashRequested -= flashHandler;
                        }
                    }

                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            if (item is IFlashRequestable vm)
                                vm.FlashRequested += flashHandler;
                        }
                    }
                };

                observable.CollectionChanged += collectionHandler;
                grid.SetValue(CollectionChangedHandlerProperty, collectionHandler);
            }
        }

        private static void Unsubscribe(DataGrid grid)
        {
            var flashHandler =
                grid.GetValue(FlashHandlerProperty) as EventHandler<RowFlashRequestedEventArgs>;

            if (flashHandler != null && grid.ItemsSource is IEnumerable items)
            {
                foreach (var item in items)
                {
                    if (item is IFlashRequestable vm)
                        vm.FlashRequested -= flashHandler;
                }
            }

            if (grid.ItemsSource is INotifyCollectionChanged observable)
            {
                var collectionHandler =
                    grid.GetValue(CollectionChangedHandlerProperty)
                    as NotifyCollectionChangedEventHandler;

                if (collectionHandler != null)
                    observable.CollectionChanged -= collectionHandler;
            }

            grid.ClearValue(FlashHandlerProperty);
            grid.ClearValue(CollectionChangedHandlerProperty);
        }

        private static async Task ScrollAndFlashAsync(DataGrid grid, object item, RowFlashRequestedEventArgs e)
        {
            if (!grid.Dispatcher.CheckAccess())
            {
                await grid.Dispatcher.InvokeAsync(
                    async () => await ScrollAndFlashAsync(grid, item, e));

                return;
            }

            grid.SelectedItem = item;
            grid.CurrentItem = item;


            grid.ScrollIntoView(item);

            await grid.Dispatcher.InvokeAsync(
                () => { },
                DispatcherPriority.Background);

            grid.UpdateLayout();

            var row = grid.ItemContainerGenerator.ContainerFromItem(item)
                as DataGridRow;

            if (row == null)
            {
                grid.ScrollIntoView(item);

                await grid.Dispatcher.InvokeAsync(
                    () => { },
                    DispatcherPriority.ContextIdle);

                grid.UpdateLayout();

                row = grid.ItemContainerGenerator.ContainerFromItem(item)
                    as DataGridRow;
            }

            if (row != null)
            {
                row.IsSelected = true;
                row.Focus(); 

                row.BringIntoView();
                Flash(row, e);
            }
            else
            {
                e.Completion.TrySetResult(false);
            }
        }

        private static void Flash(DataGridRow row, RowFlashRequestedEventArgs args)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(row);

            if (adornerLayer == null)
                return;

            var adorner = new RowFlashAdorner(row);

            Color color;

            switch (args.Method)
            {
                case SaleBoundPurchaseCommand.DTO.Type.AddNew:
                    color = Color.FromRgb(76, 175, 80);
                    break;
                case SaleBoundPurchaseCommand.DTO.Type.Add:
                    color = Color.FromRgb(255, 193, 7);
                    break;
                case SaleBoundPurchaseCommand.DTO.Type.Subtract:
                    color = Color.FromRgb(244, 67, 54);
                    break;
                default: 
                    throw new ArgumentOutOfRangeException(nameof(args.Method));
            }

            var brush = new SolidColorBrush(color);
            adorner.OverlayBrush = brush;

            adornerLayer.Add(adorner);

            var animation = new ColorAnimation
            {
                From = color,
                To = Colors.Transparent,
                Duration = TimeSpan.FromMilliseconds(450),
                FillBehavior = FillBehavior.Stop
            };

            animation.Completed += (s, e) =>
            {
                adornerLayer.Remove(adorner);
                args.Completion.TrySetResult(true);
            };

            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }
    }

    internal class RowFlashAdorner : Adorner
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

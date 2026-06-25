using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Mhyrenz_Interface.Shared.Adorners;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Shared.Behaviors
{

    public class RowFlashRequestedEventArgs : EventArgs
    {
        public RowFlashRequestedEventArgs(DataGridFlashBehavior.OperationType method)
        {
            Method = method;
            Completion = new TaskCompletionSource<bool>();
        }

        public DataGridFlashBehavior.OperationType Method { get; private set; }
        public TaskCompletionSource<bool> Completion { get; }
    }

    public static class DataGridFlashHelper
    {
        public static Task RequestFlash(this EventHandler<RowFlashRequestedEventArgs> eventHandler, IFlashReceiver item, DataGridFlashBehavior.OperationType type)
        {
            var args = new RowFlashRequestedEventArgs(type);
            eventHandler?.Invoke(item, args);

            return args.Completion.Task;
        }
    }

    public class DataGridFlashBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty WatchedItemsSourceProperty =
            DependencyProperty.Register(
                nameof(WatchedItemsSource),
                typeof(string),
                typeof(DataGridFlashBehavior),
                new PropertyMetadata(null));

        public string WatchedItemsSource
        {
            get => (string)GetValue(WatchedItemsSourceProperty);
            set => SetValue(WatchedItemsSourceProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
            HookFlasher(AssociatedObject.DataContext);
        }

        private IFlashRequestable _currentFlasher;

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;

            HookFlasher(AssociatedObject.DataContext);
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;

            UnhookFlasher();
        }

        private void AssociatedObject_DataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            UnhookFlasher();
            HookFlasher(e.NewValue);
        }

        private void HookFlasher(object dataContext)
        {
            _currentFlasher = dataContext as IFlashRequestable;

            if (_currentFlasher != null)
            {
                _currentFlasher.FlashRequested += Flasher_FlashRequested;
            }
        }

        private void UnhookFlasher()
        {
            if (_currentFlasher != null)
            {
                _currentFlasher.FlashRequested -= Flasher_FlashRequested;
                _currentFlasher = null;
            }
        }

        private async void Flasher_FlashRequested(object sender, RowFlashRequestedEventArgs e)
        {
            if (!(sender is IFlashReceiver receiver))
            {
                throw new InvalidOperationException(
                    $"Objects raising {nameof(IFlashRequestable.FlashRequested)} must implement {nameof(IFlashReceiver)}.");
            }

            await ScrollAndFlashAsync(AssociatedObject, receiver, e);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;
            UnhookFlasher();
        }

        private static async Task ScrollAndFlashAsync(DataGrid grid, object item, RowFlashRequestedEventArgs e)
        {
            if (!grid.Dispatcher.CheckAccess())
            {
                await grid.Dispatcher.InvokeAsync(async () => await ScrollAndFlashAsync(grid, item, e));
                return;
            }

            grid.SelectedItem = item;
            grid.CurrentItem = item;
            grid.ScrollIntoView(item);

            await grid.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            grid.UpdateLayout();

            var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;

            if (row == null)
            {
                grid.ScrollIntoView(item);

                await grid.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);

                grid.UpdateLayout();

                row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
            }

            if (row == null)
            {
                e.Completion.TrySetResult(false);
                return;
            }

            row.IsSelected = true;
            row.Focus();
            row.BringIntoView();

            Flash(row, e);
        }

        private static void Flash(DataGridRow row, RowFlashRequestedEventArgs args)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(row);

            if (adornerLayer == null)
            {
                args.Completion.TrySetResult(false);
                return;
            }

            var adorner = new RowFlashAdorner(row);

            Color color;

            switch (args.Method)
            {
                case OperationType.New:
                    color = Color.FromRgb(76, 175, 80);
                    break;

                case OperationType.Update:
                    color = Color.FromRgb(255, 193, 7);
                    break;

                case OperationType.Remove:
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

        public enum OperationType
        {
            New,
            Update,
            Remove
        }
    }
}

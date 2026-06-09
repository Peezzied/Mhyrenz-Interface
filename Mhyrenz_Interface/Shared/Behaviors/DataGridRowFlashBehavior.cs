using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Mhyrenz_Interface.Shared.Adorners;
using Mhyrenz_Interface.Store;
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

    public class DataGridFlashBehavior : Behavior<DataGrid>
    {
        private NotifyCollectionChangedEventHandler _collectionChangedHandler;
        private EventHandler<RowFlashRequestedEventArgs> _flashHandler;
        private PropertyChangedEventHandler _dataContextPropertyChangedHandler;

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
            base.OnAttached();

            AssociatedObject.Loaded += Grid_Loaded;
            AssociatedObject.Unloaded += Grid_Unloaded;
            AssociatedObject.DataContextChanged += Grid_DataContextChanged;

            SubscribeDataContext();
            SubscribeItems();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= Grid_Loaded;
            AssociatedObject.Unloaded -= Grid_Unloaded;
            AssociatedObject.DataContextChanged -= Grid_DataContextChanged;

            UnsubscribeDataContext(AssociatedObject.DataContext);
            UnsubscribeItems();

            base.OnDetaching();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            SubscribeDataContext();
            SubscribeItems();
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeDataContext(AssociatedObject.DataContext);
            UnsubscribeItems();
        }

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnsubscribeDataContext(e.OldValue);
            SubscribeDataContext();
            SubscribeItems();
        }

        private void SubscribeDataContext()
        {
            UnsubscribeDataContext(AssociatedObject.DataContext);

            if (!(AssociatedObject.DataContext is INotifyPropertyChanged notify))
                return;

            _dataContextPropertyChangedHandler = (sender, e) =>
            {
                if (e.PropertyName != WatchedItemsSource)
                    return;

                AssociatedObject.Dispatcher.BeginInvoke(new Action(SubscribeItems), DispatcherPriority.Loaded);
            };

            notify.PropertyChanged += _dataContextPropertyChangedHandler;
        }

        private void UnsubscribeDataContext(object dataContext)
        {
            if (_dataContextPropertyChangedHandler != null &&
                dataContext is INotifyPropertyChanged notify)
            {
                notify.PropertyChanged -= _dataContextPropertyChangedHandler;
            }

            _dataContextPropertyChangedHandler = null;
        }

        private void SubscribeItems()
        {
            UnsubscribeItems();

            if (!(AssociatedObject.ItemsSource is IEnumerable items))
                return;

            _flashHandler = async (sender, e) =>
            {
                if (sender != null)
                    await ScrollAndFlashAsync(AssociatedObject, sender, e);
            };

            foreach (var item in items)
            {
                if (item is IFlashRequestable vm)
                    vm.FlashRequested += _flashHandler;
            }

            if (AssociatedObject.ItemsSource is INotifyCollectionChanged observable)
            {
                _collectionChangedHandler = (sender, e) =>
                {
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            if (item is IFlashRequestable vm)
                                vm.FlashRequested -= _flashHandler;
                        }
                    }

                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            if (item is IFlashRequestable vm)
                                vm.FlashRequested += _flashHandler;
                        }
                    }
                };

                observable.CollectionChanged += _collectionChangedHandler;
            }
        }

        private void UnsubscribeItems()
        {
            if (_flashHandler != null &&
                AssociatedObject.ItemsSource is IEnumerable items)
            {
                foreach (var item in items)
                {
                    if (item is IFlashRequestable vm)
                        vm.FlashRequested -= _flashHandler;
                }
            }

            if (_collectionChangedHandler != null &&
                AssociatedObject.ItemsSource is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged -= _collectionChangedHandler;
            }

            _flashHandler = null;
            _collectionChangedHandler = null;
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

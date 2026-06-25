using System;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Features.Orders.Commands;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;
using ObservableCollections;
using MessageBox = HandyControl.Controls.MessageBox;
using Setter = Mhyrenz_Interface.Core.PropertyTracking.TrackPropertyHelper.Setter;

namespace Mhyrenz_Interface.Features.Orders.ViewModels
{
    public class PlaceOrderViewModel : ValidationViewModel, IFlyoutViewModel, IFlashRequestable
    {
        private readonly IOrderStore _orderStore;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly CreateCommand<PlaceOrderVMCommandQty> _placeOrderQtyCommand;
        private readonly IOrderService _orderService;

        public PlaceOrderViewModel(IOrderStore orderStore, IUndoRedoManager undoRedoManager, CreateCommand<PlaceOrderVMCommandQty> placeOrderQtyCommand, IOrderService orderService)
        {
            _orderStore = orderStore;
            _undoRedoManager = undoRedoManager;
            _placeOrderQtyCommand = placeOrderQtyCommand;
            _orderService = orderService;

            OrderView = orderStore.Store.Source.CreateView(v => v);
            Orders = OrderView.ToNotifyCollectionChanged(
                SynchronizationContextCollectionEventDispatcher.Current);

            OrderView.ViewChanged += OrderView_ViewChanged;

            OrderDropHandler = new OrderDropTarget(this, orderStore);

            PlaceOrderCommand = new AsyncRelayCommand<bool>(PlaceOrderAction, CanEmailCommand);
        }

        private string _supplier;

        public event EventHandler<RowFlashRequestedEventArgs> FlashRequested;

        [Required(
            ErrorMessage = "Supplier is required.",
            AllowEmptyStrings = false)]
        public string Supplier
        {
            get => _supplier;
            set
            {
                if (_supplier == value)
                    return;

                _supplier = value;
                OnPropertyChanged();
                Validate(nameof(Supplier), value);
            }
        }

        public void Load()
        {
            App.Current.BeginInvoke(new System.Action(() =>
            {
                foreach (var (Value, View) in OrderView.Unfiltered)
                {
                    View.TrackedPropertyChanged += OrderView_TrackedPropertyChanged;
                }
            }));
        }

        private bool CanEmailCommand(bool obj)
        {
            return !HasErrors && Orders.Count > 0;
        }

        private async Task PlaceOrderAction(bool arg)
        {
            Validate(nameof(Supplier), Supplier);

            if (HasErrors)
                return;

            if (arg)
            {
                var result = MessageBox.Show(
                "Send the purchase order to Telegram?",
                "Send Telegram Order",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                await _orderService.SaveOrdersMessage("test", "supplier");
            }
            else
            {
                var result = MessageBox.Show(
                "Generate supplier order email?",
                "Generate Email",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                await _orderService.GenerateEmail(Supplier);
            }

        }

        private void OrderView_ViewChanged(in SynchronizedViewChangedEventArgs<OrderDataViewModel, OrderDataViewModel> e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                e.NewItem.View.TrackedPropertyChanged += OrderView_TrackedPropertyChanged;
            else if (e.Action == NotifyCollectionChangedAction.Remove)
                e.OldItem.View.TrackedPropertyChanged -= OrderView_TrackedPropertyChanged;

            PlaceOrderCommand.OnCanExecuteChanged();
        }

        private void OrderView_TrackedPropertyChanged(object sender, TrackedPropertyChangedEventArgs args)
        {
            if (!args.IsTrueOrigin)
                return;

            var viewModel = sender as OrderDataViewModel;

            TrackQtyProps(args.PropertyName, viewModel.Order.ProductId, args.OldValue, args.NewValue);
        }

        public ISynchronizedView<OrderDataViewModel, OrderDataViewModel> OrderView { get; }
        public NotifyCollectionChangedSynchronizedViewList<OrderDataViewModel> Orders { get; }

        public OrderDropTarget OrderDropHandler { get; }
        public AsyncRelayCommand<bool> PlaceOrderCommand { get; }

        public string FlyoutTitle => "Place Order";

        protected TrackPropertyHelper<int, OrderDataViewModel> TrackQtyProps(string propertyName, int productId, object oldValue, object newValue)
        {
            var tracker = TrackPropertyHelper.Build(_orderStore, productId, propertyName)
                .Track(nameof(OrderDataViewModel.Qty), method);

            async void method(Setter setter, int key)
            {
                await _placeOrderQtyCommand(new PlaceOrderVMCommandQty.DTO
                {
                    Owner = this,
                    ProductId = productId,
                    ChangedArgs = new PlaceOrderVMCommandQty.ChangedArgs
                    {
                        NewValue = newValue,
                        OldValue = oldValue,
                        RowInfo = null
                    }
                }).Execute();
            }

            return tracker;
        }


        public override void Dispose()
        {
            OrderView.ViewChanged -= OrderView_ViewChanged;

            foreach (var (Value, View) in OrderView.Unfiltered)
            {
                View.TrackedPropertyChanged -= OrderView_TrackedPropertyChanged;
            }
        }

        protected override IRaiseCanExecuteChanged SubmitActionCommand()
        {
            return PlaceOrderCommand;
        }

        public async Task RequestFlash(IFlashReceiver item, DataGridFlashBehavior.OperationType type)
        {
            await FlashRequested.RequestFlash(item, type);
        }

        public class OrderDropTarget : DefaultDropHandler
        {
            private readonly PlaceOrderViewModel _placeOrderViewModel;
            private readonly IOrderStore _orderStore;

            public OrderDropTarget(PlaceOrderViewModel placeOrderViewModel, IOrderStore orderStore)
            {
                _placeOrderViewModel = placeOrderViewModel;
                _orderStore = orderStore;
            }

            public override void DragOver(IDropInfo dropInfo)
            {
                base.DragOver(dropInfo);
                if (dropInfo.Data is ProductDataViewModel)
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                    dropInfo.DropTargetAdorner = null;
                }
            }

            public override void Drop(IDropInfo dropInfo)
            {
                var product = (dropInfo.Data as ProductDataViewModel).Item;

                if (_orderStore.Store.TryGetValue(product.Id, out var order))
                {
                    order.Qty += 1;
                }
                else
                {
                    _placeOrderViewModel.TrackQtyProps(nameof(OrderDataViewModel.Qty),
                        productId: product.Id,
                        oldValue: 0,
                        newValue: 1);
                }
            }
        }
    }
}

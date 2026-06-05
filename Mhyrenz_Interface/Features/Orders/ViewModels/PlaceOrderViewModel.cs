using System.Collections.Specialized;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Features.Orders.Commands;
using Mhyrenz_Interface.Store;
using ObservableCollections;
using static Mhyrenz_Interface.Core.PropertyTracking.TrackPropertyHelper;
using Setter = Mhyrenz_Interface.Core.PropertyTracking.TrackPropertyHelper.Setter;

namespace Mhyrenz_Interface.Features.Orders.ViewModels
{
    public class PlaceOrderViewModel : BaseViewModel
    {
        private readonly IOrderStore _orderStore;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly CreateCommand<PlaceOrderVMCommandQty> _placeOrderQtyCommand;

        public PlaceOrderViewModel(IOrderStore orderStore, IUndoRedoManager undoRedoManager, CreateCommand<PlaceOrderVMCommandQty> placeOrderQtyCommand)
        {
            _orderStore = orderStore;
            _undoRedoManager = undoRedoManager;
            _placeOrderQtyCommand = placeOrderQtyCommand;

            OrderView = orderStore.Store.Source.CreateView(v => v);
            Orders = OrderView.ToNotifyCollectionChanged();

            OrderView.ViewChanged += OrderView_ViewChanged;

            foreach (var (Value, View) in OrderView.Unfiltered)
            {
                View.TrackedPropertyChanged += OrderView_TrackedPropertyChanged;
            }

            OrderDropHandler = new OrderDropTarget(this, orderStore);
        }


        private void OrderView_ViewChanged(in SynchronizedViewChangedEventArgs<OrderViewModel, OrderViewModel> e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                e.NewItem.View.TrackedPropertyChanged += OrderView_TrackedPropertyChanged;
            else if (e.Action == NotifyCollectionChangedAction.Remove)
                e.OldItem.View.TrackedPropertyChanged -= OrderView_TrackedPropertyChanged;
        }

        private void OrderView_TrackedPropertyChanged(object sender, TrackedPropertyChangedEventArgs args)
        {
            if (!args.IsTrueOrigin)
                return;

            var viewModel = sender as OrderViewModel;

            TrackQtyProps(args.PropertyName, viewModel.Order.ProductId, args.OldValue);
        }

        public ISynchronizedView<OrderViewModel, OrderViewModel> OrderView { get; }
        public NotifyCollectionChangedSynchronizedViewList<OrderViewModel> Orders { get; }

        public OrderDropTarget OrderDropHandler { get; }

        public TrackPropertyHelper<int, OrderViewModel> TrackQtyProps(string propertyName, int productId, object oldValue, object newValue = null)
        {
            var tracker = TrackPropertyHelper.Build(_orderStore, productId, propertyName)
                .Track(nameof(OrderViewModel.Qty), method);

            void method(Setter setter, Getter getter, int key)
            {
                _undoRedoManager.Execute(_placeOrderQtyCommand(new PlaceOrderVMCommandQty.DTO
                {
                    // TODO
                }));
            }

            return tracker;
        }


        public override void Dispose()
        {
            OrderView.ViewChanged -= OrderView_ViewChanged;
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

                }
            }
        }
    }
}

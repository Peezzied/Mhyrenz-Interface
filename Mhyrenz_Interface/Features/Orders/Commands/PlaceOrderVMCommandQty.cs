using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Shared.Behaviors;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Orders.Commands
{
    public class PlaceOrderVMRowInfo
    {
        // TODO PlaceOrderVMRowInfo
    }

    public class PlaceOrderVMCommandQty : PropertyChangeCommand<PlaceOrderVMRowInfo>
    {
        private readonly DTO _dto;
        private readonly IOrderService _orderService;
        private readonly IOrderStore _orderStore;
        private Order _result;

        public PlaceOrderVMCommandQty(DTO dto, IOrderService orderService, IOrderStore orderStore) : base(dto, null)
        {
            _dto = dto;
            _orderService = orderService;
            _orderStore = orderStore;
        }

        public override async Task Command()
        {
            await base.Command();

            var newValue = PropertyChangedArgs.NewValue as int? ?? 0;
            var oldValue = PropertyChangedArgs.OldValue as int? ?? 0;

            if (newValue == oldValue)
                return;

            var amount = Math.Abs(oldValue - newValue);

            var isIncrease = newValue > oldValue;

            if (isIncrease)
            {
                _result = await _orderService.AddItem(_dto.ProductId, amount);
            }
            else
            {
                _result = await _orderService.SubtractItem(_dto.ProductId, amount);
            }

            if (_dto.Owner is IFlashRequestable flasher)
            {
                if (_result == null)
                {
                    if (!_orderStore.Store.TryGetValue(_dto.ProductId, out var item))
                        return;

                    await flasher.RequestFlash(item, DataGridFlashBehavior.OperationType.Remove);
                    _orderStore.Store.Remove(_dto.ProductId);
                    return;
                }

                if (_orderStore.Store.TryGetValue(_dto.ProductId, out var existing))
                {
                    existing.Order = _result;
                    await flasher.RequestFlash(existing, DataGridFlashBehavior.OperationType.Update);
                    return;
                }

                var vm = _orderStore.AddItem(_result);
                _ = App.Current.Dispatcher.BeginInvoke(new Action(() => flasher.RequestFlash(vm, DataGridFlashBehavior.OperationType.New)));
            }
            else
            {
                Cancel = true;
            }
        }

        public new class DTO : PropertyChangeCommand<PlaceOrderVMRowInfo>.DTO
        {
            public IFlashRequestable Owner { get; set; } // FIXME Temporary property
            public int ProductId { get; set; }
        }
    }
}

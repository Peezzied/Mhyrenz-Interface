using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
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

        public PlaceOrderVMCommandQty(DTO dto, IOrderService orderService, IOrderStore orderStore) : base(dto)
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

            Order result;
            if (isIncrease)
            {
                result = await _orderService.AddItem(_dto.ProductId, amount);
            }
            else
            {
                result = await _orderService.SubtractItem(_dto.ProductId, amount);
            }

            _orderStore.AddItem(result, _dto.ProductId);
        }

        public new class DTO : PropertyChangeCommand<PlaceOrderVMRowInfo>.DTO
        {
            public int ProductId { get; set; }
        }
    }
}

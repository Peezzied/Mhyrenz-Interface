using System;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Core.UndoRedo;

namespace Mhyrenz_Interface.Features.Orders.Commands
{
    public class PlaceOrderVMRowInfo
    {
        // TODO PlaceOrderVMRowInfo
    }

    public class PlaceOrderVMCommandQty : PropertyChangeCommand<PlaceOrderVMRowInfo>
    {
        private readonly DTO _dto;

        public PlaceOrderVMCommandQty(DTO dto) : base(dto)
        {
            _dto = dto;
        }

        public override void Command(object parameter, ActionType intent)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core;

namespace Mhyrenz_Interface.Commands
{
    public class PlaceOrderVMRowInfo
    {
        // TODO PlaceOrderVMRowInfo
    }

    public class PlaceOrderVMCommandQty : PropertyChangeCommand<PlaceOrderVMRowInfo>
    {
        private DTO _dto;

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core;

namespace Mhyrenz_Interface.Commands
{
    public class TransactionVMDiscountCommand : PropertyChangeCommand<TransactionVMRowInfo>
    {
        public TransactionVMDiscountCommand(DTO dto) : base(dto)
        {
        }

        public override void Command(object parameter, ActionType intent)
        {
            throw new NotImplementedException();
        }
    }
}

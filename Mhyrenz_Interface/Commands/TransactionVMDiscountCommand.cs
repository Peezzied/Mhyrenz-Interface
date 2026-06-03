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
        public TransactionVMDiscountCommand(ChangedArgs args, TrackPropertyHelper.Setter setter, Action propertyChangeHandler, Type currentViewIn) : 
            base(args, setter, propertyChangeHandler, currentViewIn)
        {
        }

        public override bool Command(object parameter, ActionType intent)
        {
            throw new NotImplementedException();
        }
    }
}

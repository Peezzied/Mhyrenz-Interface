using System;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Shared.Behaviors
{
    public interface IFlashRequestable
    {
        event EventHandler<RowFlashRequestedEventArgs> FlashRequested;
        Task RequestFlash(DataGridFlashBehavior.OperationType type);
    }
}

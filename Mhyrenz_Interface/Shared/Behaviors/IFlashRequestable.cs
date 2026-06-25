using System;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Shared.Behaviors
{
    public interface IFlashReceiver { }

    public interface IFlashRequestable
    {
        event EventHandler<RowFlashRequestedEventArgs> FlashRequested;
        Task RequestFlash(IFlashReceiver item, DataGridFlashBehavior.OperationType type);
    }
}

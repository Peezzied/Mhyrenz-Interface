using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Shared.Behaviors;

namespace Mhyrenz_Interface.Core.MVVM
{
    public class DataGridHostViewModel : BaseViewModel, IFlashRequestable
    {
        public event EventHandler<RowFlashRequestedEventArgs> FlashRequested;

        public Task RequestFlash(IFlashReceiver item, DataGridFlashBehavior.OperationType type)
        {
            var args = new RowFlashRequestedEventArgs(type);
            FlashRequested?.Invoke(item, args);

            return args.Completion.Task;
        }
    }
}


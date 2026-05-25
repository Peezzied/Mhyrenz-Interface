using System.Windows.Input;
using Mhyrenz_Interface.Core;

namespace Mhyrenz_Interface.Commands
{
    public interface IUndoRedoBound : ICommandAsync, ICommand
    {
        bool AllowBack { get; }

        void Undo(object parameter);
        void Redo(object parameter);
        void ExecuteRaw(object parameter);
    }
}

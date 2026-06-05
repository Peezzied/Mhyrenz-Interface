using System.Windows.Input;
using Mhyrenz_Interface.Core.MVVM;

namespace Mhyrenz_Interface.Core.UndoRedo
{
    public interface IUndoRedoBound : ICommandAsync, ICommand
    {
        bool AllowBack { get; }

        void Undo(object parameter);
        void Redo(object parameter);
        void ExecuteRaw(object parameter);
    }
}

using System.Windows.Input;

namespace Mhyrenz_Interface.Commands
{
    public interface IUndoableCommand
    {
        void Execute();
        void Undo();
        void Redo();
    }
}
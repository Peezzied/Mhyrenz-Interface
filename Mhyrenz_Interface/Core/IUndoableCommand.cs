using System.Windows.Input;

namespace Mhyrenz_Interface.Core
{
    public interface IUndoableCommand
    {
        void Execute();
        void Undo();
        void Redo();
    }
}
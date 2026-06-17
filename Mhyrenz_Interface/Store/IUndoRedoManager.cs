using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.UndoRedo;

namespace Mhyrenz_Interface.Store
{
    public interface IUndoRedoManager
    {
        bool CanUndo { get; }
        bool CanRedo { get; }

        event Action<ActionType, UndoRedoEventArgs> UndoRedoEvent;
        event EventHandler UndoRedoChanged;

        void Clear();
        void Execute(IUndoableCommand command);
        bool Push(IUndoableCommand command);
        Task Redo();
        Task Undo();
    }
}
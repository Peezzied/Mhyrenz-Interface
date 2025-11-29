using System;
using Mhyrenz_Interface.Core;

namespace Mhyrenz_Interface.State
{
    public interface IUndoRedoManager
    {
        bool CanUndo { get; }
        bool CanRedo { get; }

        event Action<ActionType, UndoRedoEventArgs> UndoRedoEvent;

        void Clear();
        void Execute(IUndoableCommand command);
        bool Push(IUndoableCommand command);
        void Redo();
        bool ShowWarning(Action rejectEffect = null);
        void Undo();
    }
}
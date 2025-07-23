using Mhyrenz_Interface.Core;
using System;

namespace Mhyrenz_Interface.State
{
    public interface IUndoRedoManager
    {
        bool CanUndo { get; }
        bool CanRedo { get; }

        event Action<ActionType, UndoRedoEventArgs> UndoRedoEvent;

        void Clear();
        void Execute(IUndoableCommand command);
        void Push(IUndoableCommand command);
        void Redo();
        void Undo();
    }
}
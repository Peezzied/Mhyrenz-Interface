using System;
using Mhyrenz_Interface.Navigation;

namespace Mhyrenz_Interface.Core.UndoRedo
{
    public enum ActionType
    {
        Normal, Undo, Redo
    }

    public interface IUndoableCommand
    {
        PostNavigation SideEffect { get; }
        Type CurrentViewIn { get; }
        void Execute();
        bool Undo();
        bool Redo();
    }
}
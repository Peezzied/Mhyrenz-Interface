using Mhyrenz_Interface.ViewModels.Factory;
using System;
using System.Windows.Input;

namespace Mhyrenz_Interface.Core
{
    public enum ActionType
    {
        Normal, Undo, Redo
    }

    public interface IUndoableCommand
    {
        Action<NavigationViewModel> SideEffect { get; }
        Type CurrentViewIn { get; }
        void Execute();
        bool Undo();
        bool Redo();
    }
}
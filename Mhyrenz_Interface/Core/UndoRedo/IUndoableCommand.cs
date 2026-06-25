using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Navigation;

namespace Mhyrenz_Interface.Core.UndoRedo
{
    public enum ActionType
    {
        Normal, Undo, Redo
    }

    public interface IUndoableCommand
    {
        PostNavigation Completer { get; }
        Type Context { get; }
        ActionType Intent { get; set; }
        bool Cancel { get; set; }

        Task Execute();
        Task Undo();
        Task Redo();
    }
}
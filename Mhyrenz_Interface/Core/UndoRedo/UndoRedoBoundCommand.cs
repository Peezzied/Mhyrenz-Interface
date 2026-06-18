using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Core.UndoRedo
{

    public abstract class UndoRedoBoundCommand : IUndoableCommand
    {
        public PostNavigation Completer { get; set; }
        public Type CurrentViewIn { get; }
        public ActionType Intent { get; set; } = ActionType.Normal;
        public bool Cancel { get; set; } = false;

        public UndoRedoBoundCommand(Type view)
        {
            CurrentViewIn = view;
        }

        public abstract Task Command();

        public async Task Execute()
        {
            await Command();
        }

        public async Task Undo()
        {
            Intent = ActionType.Undo;
            await Command();
        }

        public async Task Redo()
        {
            Intent = ActionType.Redo;
            await Command();
        }
    }
}

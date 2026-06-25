using System;
using System.Threading.Tasks;
using Mhyrenz_Interface.Navigation;

namespace Mhyrenz_Interface.Core.UndoRedo
{

    public abstract class UndoRedoBoundCommand : IUndoableCommand
    {
        public PostNavigation Completer { get; set; }
        public Type Context { get; }
        public ActionType Intent { get; set; } = ActionType.Normal;
        public bool Cancel { get; set; } = false;

        public UndoRedoBoundCommand(Type view)
        {
            Context = view;
        }

        public abstract Task Command();

        protected virtual async Task Complete() { }

        public async Task Execute()
        {
            await Command();
        }

        public async Task Undo()
        {
            Cancel = false;
            Intent = ActionType.Undo;
            await Command();
        }

        public async Task Redo()
        {
            Cancel = false;
            Intent = ActionType.Redo;
            await Command();
        }
    }
}

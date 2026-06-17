using System;
using Mhyrenz_Interface.Navigation;

namespace Mhyrenz_Interface.Core.UndoRedo
{

    public class UndoRedoBoundCommand : IUndoableCommand
    {
        public IUndoRedoBound Command { get; private set; }
        public PostNavigation SideEffect { get; }
        public Type CurrentViewIn { get; }

        private readonly object _commandParameter;

        public UndoRedoBoundCommand(IUndoRedoBound command, PostNavigation sideEffect, Type view, object commandParameter = null)
        {
            Command = command;
            SideEffect = sideEffect;
            CurrentViewIn = view;
            _commandParameter = commandParameter;
        }


        public void Execute()
        {
            Command.Execute(_commandParameter);
        }

        public bool Redo()
        {
            Command.Redo(_commandParameter);
            return Command.AllowBack;
        }

        public bool Undo()
        {
            Command.Undo(_commandParameter);
            return Command.AllowBack;
        }
    }
}

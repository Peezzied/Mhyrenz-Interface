using System;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.Commands
{
   
    public class UndoRedoBoundCommand : IUndoableCommand
    {
        public IUndoRedoBound Command { get; private set; }
        public Action<NavigationViewModel> SideEffect { get; }
        public Type CurrentViewIn { get; }

        private readonly object _commandParameter;

        public UndoRedoBoundCommand(IUndoRedoBound command, Action<NavigationViewModel> sideEffect, Type view, object commandParameter = null)
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

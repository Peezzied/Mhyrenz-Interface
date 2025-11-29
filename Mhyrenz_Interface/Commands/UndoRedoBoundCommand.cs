using System;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Mhyrenz_Interface.Commands
{
    public class UndoRedoBoundCommand : IUndoableCommand
    {
        private readonly IUndoRedoBound _command;

        public Action<NavigationViewModel> SideEffect { get; }
        public Type CurrentViewIn { get; }

        private readonly object _commandParameter;
        private readonly IUndoRedoManager _undoRedoManager = App.ServiceProvider.GetRequiredService<IUndoRedoManager>();

        public UndoRedoBoundCommand(IUndoRedoBound command, Action<NavigationViewModel> sideEffect, Type view, object commandParameter = null)
        {
            _command = command;
            SideEffect = sideEffect;
            CurrentViewIn = view;
            _commandParameter = commandParameter;
        }


        public void Execute()
        {
            _command.Execute(_commandParameter);
        }

        public bool Redo()
        {
            _command.Redo(_commandParameter);
            return _command.AllowBack;
        }

        public bool Undo()
        {
            _command.Undo(_commandParameter);
            return _command.AllowBack;
        }
    }
}

using Mhyrenz_Interface.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.Commands
{
    public class UndoRedoBoundCommand : IUndoableCommand
    {
        private readonly IUndoRedoBound _command;

        public Type CurrentViewIn { get; private set; }

        private readonly object _commandParameter;

        public UndoRedoBoundCommand(IUndoRedoBound command, Type currentViewIn, object commandParameter = null)
        {
            _command = command;
            CurrentViewIn = currentViewIn;
            _commandParameter = commandParameter;
        }

        public void Execute()
        {
            _command.Execute(_commandParameter);
        }

        public void Redo()
        {
            _command.Redo(_commandParameter);
        }

        public void Undo()
        {
            _command.Undo(_commandParameter);
        }
    }
}

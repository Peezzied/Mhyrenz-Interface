using HandyControl.Controls;
using HandyControl.Data;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.ViewModels.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Mhyrenz_Interface.State
{
    public class UndoRedoEventArgs
    {
        public NavigationViewModel CurrentView { get; set; }
    }

    public class UndoRedoManager : IUndoRedoManager
    {

        private readonly Stack<IUndoableCommand> _undoStack = new Stack<IUndoableCommand>();
        private readonly Stack<IUndoableCommand> _redoStack = new Stack<IUndoableCommand>();
        private readonly INavigationServiceEx _navigationService;

        public UndoRedoManager(INavigationServiceEx navigationServiceEx)
        {
            _navigationService = navigationServiceEx;
        }

        public void Execute(IUndoableCommand command)
        {
            if (Push(command))
                command.Execute();
        }

        public bool Push(IUndoableCommand command)
        {
            _undoStack.Push(command);
            _redoStack.Clear();
            return true;
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Peek();
                if (command.Undo())
                {
                    command = _undoStack.Pop();
                    _redoStack.Push(command);
                    RaiseUndoRedoEvent(ActionType.Undo, command);
                }
            }
        }


        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Peek();
                if (command.Redo())
                {
                    command = _redoStack.Pop();
                    _undoStack.Push(command);
                    RaiseUndoRedoEvent(ActionType.Redo, command);
                }
            }
        }
        private void RaiseUndoRedoEvent(ActionType intent, IUndoableCommand command)
        {

            if (command.SideEffect is null)
            {
                _navigationService.Navigate(command.CurrentViewIn);
            } else
            {
                _navigationService.Navigate(command.CurrentViewIn, vm => command.SideEffect(vm));
            }

            App.Current.Dispatcher.BeginInvoke(new Action(() => UndoRedoEvent?.Invoke(intent, new UndoRedoEventArgs
            {
                CurrentView = _navigationService.CurrentViewModel,
            })));

        }

        public void Clear()
        {
            _redoStack.Clear();
            _undoStack.Clear();
        }

        public event Action<ActionType, UndoRedoEventArgs> UndoRedoEvent;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
    }

}

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

    public class UndoRedoManager: IUndoRedoManager
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
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear(); // Clear redo stack after new action
        }

        public void Push(IUndoableCommand command)
        {
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
                RaiseUndoRedoEvent(ActionType.Undo, command);
            }
        }


        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();
                command.Redo();
                _undoStack.Push(command);
                RaiseUndoRedoEvent(ActionType.Redo, command);
            }
        }
        private void RaiseUndoRedoEvent(ActionType intent, IUndoableCommand command)
        {
            void UndoRedoInvoke()
            {
                App.Current.Dispatcher.BeginInvoke(new Action(() => UndoRedoEvent?.Invoke(intent, new UndoRedoEventArgs
                {
                    CurrentView = _navigationService.CurrentViewModel,
                })), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }

            if (_navigationService.Navigate(command.CurrentViewIn))
            {
                App.Current.Dispatcher.BeginInvoke(new Action(UndoRedoInvoke), System.Windows.Threading.DispatcherPriority.ContextIdle);
                return;
            }
            UndoRedoInvoke();

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

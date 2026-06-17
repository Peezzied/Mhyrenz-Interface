using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Navigation;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface.Store
{
    public class UndoRedoEventArgs
    {
        public NavigationViewModel CurrentView { get; internal set; }
        public IUndoableCommand Command { get; internal set; }
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
            if (!Push(command))
                return;

            command.Execute();

            UndoRedoChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool Push(IUndoableCommand command)
        {
            _undoStack.Push(command);
            _redoStack.Clear();

            return true;
        }

        public async Task Undo()
        {
            if (_undoStack.Count == 0)
                return;

            var command = _undoStack.Peek();

            if (!command.Undo())
                return;

            command = _undoStack.Pop();
            _redoStack.Push(command);

            await RaiseUndoRedoEvent(ActionType.Undo, command);

            UndoRedoChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task Redo()
        {
            if (_redoStack.Count == 0)
                return;

            var command = _redoStack.Peek();

            if (!command.Redo())
                return;

            command = _redoStack.Pop();
            _undoStack.Push(command);

            await RaiseUndoRedoEvent(ActionType.Redo, command);

            UndoRedoChanged?.Invoke(this, EventArgs.Empty);
        }

        private async Task RaiseUndoRedoEvent(ActionType intent, IUndoableCommand command)
        {
            await _navigationService.NavigateAsync(command.CurrentViewIn);

            command.SideEffect?.Invoke(
                _navigationService.CurrentViewModel);

            UndoRedoEvent?.Invoke(
                intent,
                new UndoRedoEventArgs
                {
                    CurrentView = _navigationService.CurrentViewModel,
                    Command = command
                });
        }

        public void Clear()
        {
            _redoStack.Clear();
            _undoStack.Clear();
        }

        public static bool ShowWarning(Action rejectEffect = null)
        {
            MessageBoxResult prompt = MessageBox.Show("Are you sure you want to proceed with the action after the changes you've made?",
                        "Action warning",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Warning);

            if (prompt == MessageBoxResult.Cancel || prompt == MessageBoxResult.No)
            {
                rejectEffect?.Invoke();
                return false;
            }

            return true;
        }

        public event Action<ActionType, UndoRedoEventArgs> UndoRedoEvent;

        public event EventHandler UndoRedoChanged;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
    }

}

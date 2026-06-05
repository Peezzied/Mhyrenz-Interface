using System;
using System.Collections.Generic;
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
            }
            else
            {
                _navigationService.Navigate(command.CurrentViewIn, vm => command.SideEffect(vm));
            }

            App.Current.Dispatcher.BeginInvoke(new Action(() => UndoRedoEvent?.Invoke(intent, new UndoRedoEventArgs
            {
                CurrentView = _navigationService.CurrentViewModel,
                Command = command
            })));

        }

        public void Clear()
        {
            _redoStack.Clear();
            _undoStack.Clear();
        }

        public bool ShowWarning(Action rejectEffect = null)
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

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
    }

}

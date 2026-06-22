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

    public class UndoRedoInfo
    {
        public bool Cancel { get; set; } = false;
        public ActionType Type { get; set; } = ActionType.Normal;
    }

    public class UndoRedoManager : IUndoRedoManager
    {
        private readonly List<IUndoableCommand> _undoList = new List<IUndoableCommand>();
        private readonly List<IUndoableCommand> _redoList = new List<IUndoableCommand>();
        private readonly INavigationServiceEx _navigationService;

        public UndoRedoManager(INavigationServiceEx navigationServiceEx)
        {
            _navigationService = navigationServiceEx;
        }

        public async Task Execute(IUndoableCommand command)
        {
            if (CanRedo && CanUndo)
            {
                if (!ShowWarning())
                    return;
            }

            await command.Execute();

            if (command.Cancel)
                return;

            _undoList.Add(command);
            _redoList.Clear();

            UndoRedoChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task Undo()
        {
            if (_undoList.Count == 0)
                return;

            var index = _undoList.Count - 1;
            var command = _undoList[index];

            await command.Undo();

            if (command.Cancel)
                return;

            _undoList.RemoveAt(index);
            _redoList.Add(command);

            await RaiseUndoRedoEvent(ActionType.Undo, command);

            UndoRedoChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task Redo()
        {
            if (_redoList.Count == 0)
                return;

            var index = _redoList.Count - 1;
            var command = _redoList[index];

            await command.Redo();

            if (command.Cancel)
                return;

            _redoList.RemoveAt(index);
            _undoList.Add(command);

            await RaiseUndoRedoEvent(ActionType.Redo, command);

            UndoRedoChanged?.Invoke(this, EventArgs.Empty);
        }

        private async Task RaiseUndoRedoEvent(ActionType intent, IUndoableCommand command)
        {
            await _navigationService.NavigateAsync(command.Context);

            if (command.Completer != null)
            {
                await App.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await command.Completer(_navigationService.CurrentViewModel);
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            }

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
            _redoList.Clear();
            _undoList.Clear();
        }

        public void RemoveAll(Predicate<IUndoableCommand> match)
        {
            _undoList.RemoveAll(match);
            _redoList.RemoveAll(match);

            UndoRedoChanged?.Invoke(this, EventArgs.Empty);
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

        public bool CanUndo => _undoList.Count > 0;
        public bool CanRedo => _redoList.Count > 0;
    }

}

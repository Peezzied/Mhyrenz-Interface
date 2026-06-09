using System;
using System.Windows.Input;

namespace Mhyrenz_Interface.Core.MVVM
{
    public abstract class BaseCommand : ICommand, IRaiseCanExecuteChanged
    {
        private EventHandler _canExecuteChanged;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                _canExecuteChanged += value;
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                _canExecuteChanged -= value;
                CommandManager.RequerySuggested -= value;
            }
        }

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

        public virtual void OnCanExecuteChanged()
        {
            _canExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}

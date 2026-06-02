using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.Core
{
    public delegate TCommand CreateCommand<out TCommand>(params object[] parameters);
    public abstract class BaseAsyncCommand : ICommandAsync, IRaiseCanExecuteChanged
    {
        private bool _isExecuting;
        public bool IsExecuting
        {
            get
            {
                return _isExecuting;
            }
            set
            {
                _isExecuting = value;
                OnCanExecuteChanged();
            }
        }

        public event EventHandler CanExecuteChanged;

        public virtual bool CanExecute(object parameter)
        {
            return !IsExecuting;
        }

        public virtual async void Execute(object parameter)
        {
            IsExecuting = true;

            await ExecuteAsync(parameter);

            IsExecuting = false;
        }

        public abstract Task ExecuteAsync(object parameter);

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }

    public interface IRaiseCanExecuteChanged
    {
        void OnCanExecuteChanged();
    }

    public interface ICommandAsync : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}

using System.Threading.Tasks;

namespace Mhyrenz_Interface.Core.MVVM
{
    public delegate TCommand CreateCommand<out TCommand>(params object[] parameters);

    public abstract class BaseAsyncCommand : BaseCommand
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

        public override bool CanExecute(object parameter)
        {
            return !IsExecuting;
        }

        public override async void Execute(object parameter)
        {
            IsExecuting = true;

            await ExecuteAsync(parameter);

            IsExecuting = false;
        }

        public abstract Task ExecuteAsync(object parameter);
    }
}

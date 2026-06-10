using System;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Core.MVVM
{

    public class RelayCommand<T> : BaseCommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) =>
            _canExecute == null || _canExecute((T)parameter);

        public override void Execute(object parameter) =>
            _execute((T)parameter);
    }

    public class RelayCommand : BaseCommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) =>
            _canExecute == null || _canExecute(parameter);

        public override void Execute(object parameter) =>
            _execute(parameter);
    }

    public class AsyncRelayCommand : BaseAsyncCommand
    {
        private readonly Func<object, Task> _execute;
        private readonly Predicate<object> _canExecute;

        public AsyncRelayCommand(Func<object, Task> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) &&
                (_canExecute == null || _canExecute(parameter));
        }

        public override async Task ExecuteAsync(object parameter)
        {
            await _execute(parameter);
        }
    }

    public class AsyncRelayCommand<T> : BaseAsyncCommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Predicate<T> _canExecute;

        public AsyncRelayCommand(Func<T, Task> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) &&
                (_canExecute == null || _canExecute((T)parameter));
        }

        public override async Task ExecuteAsync(object parameter)
        {
            await _execute((T)parameter);
        }
    }
}

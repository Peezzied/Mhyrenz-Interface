using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.Validation;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Startup.ViewModels
{
    public class SessionBoxContext : ValidationViewModel<Session>
    {
        private readonly ISessionStore _sessionStore;

        private DateTime _session = DateTime.Now.AddDays(-1);

        [Required]
        public DateTime Session
        {
            get => _session;
            set
            {
                _session = value;
                Validate(nameof(Session), value);
                OnPropertyChanged(nameof(Session));
            }
        }

        private bool _validationHasError;
        [MustBeFalse]
        public bool ValidationHasError
        {
            get => _validationHasError;
            set
            {
                _validationHasError = value;
                Validate(nameof(ValidationHasError), value);
                OnPropertyChanged(nameof(ValidationHasError));
            }
        }

        private bool _isSessionBox = false;
        public bool IsSessionBox
        {
            get => _isSessionBox;
            set
            {
                _isSessionBox = value;
                OnPropertyChanged(nameof(IsSessionBox));
            }
        }

        public string Text => "New Session";


        public ICommand CloseButtonCommand { get; set; }
        public BaseAsyncCommand OkButtonCommand { get; set; }
        public SessionBoxContext(ISessionStore sessionStore, CreateCommand<CreateSessionCommand> createSessionCommand)
        {
            _sessionStore = sessionStore;

            OkButtonCommand = createSessionCommand(this);
            CloseButtonCommand = new RelayCommand(CloseActionCommand);
        }

        public event Action SessionCreated;

        public void RaiseSessionCreate()
        {
            SessionCreated?.Invoke();
        }

        private void CloseActionCommand(object obj)
        {
            IsSessionBox = false;
            base.InvokeClearValidations();
        }

        protected override IRaiseCanExecuteChanged SubmitActionCommand()
        {
            return OkButtonCommand;
        }
    }
}

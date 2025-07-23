using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Core.ValidationAttributes;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Domain.State;
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace Mhyrenz_Interface.ViewModels
{
    public class SessionBoxContext : ValidationViewModel
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
        public SessionBoxContext(ISessionStore sessionStore, ISessionService sessionService)
        {
            _sessionStore = sessionStore;

            OkButtonCommand = new CreateSessionCommand(this, sessionService, sessionStore);
            CloseButtonCommand = new RelayCommand(CloseActionCommand);

            base.SubmitActionCommand = OkButtonCommand;
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
    }
}

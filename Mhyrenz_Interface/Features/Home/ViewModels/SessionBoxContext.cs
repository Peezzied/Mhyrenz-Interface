using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Interactivity;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.Validation;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Store;
using Newtonsoft.Json.Linq;

namespace Mhyrenz_Interface.Features.Home.ViewModels
{
    public class SessionBoxContext : ValidationViewModel<Session>
    {
        private readonly ISessionStore _sessionStore;
        private readonly ISessionService _sessionService;
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

        public ICommand CloseButtonCommand { get; set; }
        public BaseAsyncCommand OkButtonCommand { get; set; }
        public SessionBoxContext(ISessionStore sessionStore, ISessionService sessionService)
        {
            _sessionStore = sessionStore;
            _sessionService = sessionService;
            OkButtonCommand = new AsyncRelayCommand(OkButtonActionCommand, CanOkButtonCommand);
            CloseButtonCommand = new RelayCommand(CloseActionCommand);
        }

        private async Task OkButtonActionCommand(object arg)
        {
            var target = Keyboard.FocusedElement;

            OkButtonCommand.OnCanExecuteChanged();
            Validate(nameof(Session), Session);
            OkButtonCommand.OnCanExecuteChanged();

            if (HasErrors)
                return;

            MessageBoxResult result = HandyControl.Controls.MessageBox.Show(
                    $"Create a new session for {Session:D}?",
                    "Create Session",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);


            if (result != MessageBoxResult.Yes)
                return;

            _sessionStore.CurrentSession =
                await _sessionService.GenerateSession(
                    new Session
                    {
                        Period = Session
                    });

            ControlCommands.Close.Execute(null, target);
            base.InvokeClearValidations();

            Growl.Success(new GrowlInfo
            {
                Message = $"Session \"{_sessionStore.CurrentSession.Period:D}\" has been created successfully!",
                ShowDateTime = false,
            });

        }

        private bool CanOkButtonCommand(object obj)
        {
            return !HasErrors;
        }

        public event Action SessionCreated;

        public void RaiseSessionCreate()
        {
            SessionCreated?.Invoke();
        }

        private void CloseActionCommand(object obj)
        {
            ControlCommands.Close.Execute(null, null);
            base.InvokeClearValidations();
        }

        protected override IRaiseCanExecuteChanged SubmitActionCommand()
        {
            return OkButtonCommand;
        }
    }
}

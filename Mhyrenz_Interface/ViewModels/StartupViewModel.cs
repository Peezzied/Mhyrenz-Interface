using HandyControl.Controls;
using HandyControl.Data;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Core.ValidationAttributes;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.ViewModels
{
    public class StartupViewModel : BaseViewModel, SalesRegisterHost
    {
        private readonly ISessionService _sessionService;
        private readonly ISessionStore _sessionStore;
        private readonly ITransactionsService _transactionService;
        private readonly IInventoryStore _inventoryStore;
        private readonly ITransactionStore _transactionStore;

        public SessionBoxContext SessionBoxContext { get; set; }
        public ICommand NewButtonCommand { get; set; }
        public SalesRegisterCommand RegisterCommand { get; private set; }
        public ICommand DeleteSessionCommand { get; set; }
        public ICommand StartSessionCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand SaveEditCommand { get; set; }

        private bool _isRegistering;
        public bool IsRegistering
        {
            get => _isRegistering;
            set
            {
                _isRegistering = value;
                OnPropertyChanged(nameof(IsRegistering));
            }
        }
        private bool _canCreateSession;
        public bool CanCreateSession
        {
            get => _canCreateSession;
            set
            {
                _canCreateSession = value;
                OnPropertyChanged(nameof(CanCreateSession));
            }
        }

        private bool _canStartSession;
        public bool CanStartSession
        {
            get => _canStartSession;
            set
            {
                _canStartSession = value;
                OnPropertyChanged(nameof(CanStartSession));
            }
        }

        private bool _hasTransactions;
        public bool HasTransactions
        {
            get => _hasTransactions;
            set
            {
                _hasTransactions = value;
                OnPropertyChanged(nameof(HasTransactions));
            }
        }

        private bool _showCalendar = false;
        public bool ShowCalendar
        {
            get => _showCalendar;
            set
            {
                _showCalendar = value;
                OnPropertyChanged(nameof(ShowCalendar));
            }
        }

        private DateTime _editCaledarDate;
        public DateTime EditCalendarDate
        {
            get => _editCaledarDate;
            set
            {
                _editCaledarDate = value;
                OnPropertyChanged(nameof(EditCalendarDate));
            }
        }

        public StartupViewModel(CreateViewModel<SessionBoxContext> sessionBoxVmFactory,
            ISessionService sessionService,
            ISalesRecordService salesRecordService,
            ITransactionStore transactionStore,
            ITransactionsService transactionsService,
            ISessionStore sessionStore,
            IInventoryStore inventoryStore)
        {
            _sessionService = sessionService;
            _sessionStore = sessionStore;
            _transactionService = transactionsService;
            _inventoryStore = inventoryStore;
            _transactionStore = transactionStore;

            HasTransactions = false;
            CanCreateSession = true;

            SessionBoxContext = sessionBoxVmFactory();
            SessionBoxContext.SessionCreated += SessionBoxContext_SessionCreated;

            NewButtonCommand = new RelayCommand(NewButtonActionCommand);
            RegisterCommand = new SalesRegisterCommand(this, salesRecordService, transactionStore, transactionsService, sessionStore, inventoryStore);
            RegisterCommand.Error += RegisterCommand_Error;

            EditCommand = new RelayCommand(EditSessionActionCommand);
            SaveEditCommand = new AsyncRelayCommand(SaveEditActionCommand);
            DeleteSessionCommand = new AsyncRelayCommand(DeleteSessionActionCommand);
            StartSessionCommand = new AsyncRelayCommand(StartSessionActionCommand);
        }

        private async Task StartSessionActionCommand(object obj)
        {
            await App.Presenter.ShowMainWindowAsync();
        }

        public override void Dispose()
        {
            SessionBoxContext.SessionCreated -= SessionBoxContext_SessionCreated;
        }
        private void RegisterCommand_Error()
        {
            Growl.Ask(new GrowlInfo
            {
                Message = "Would you like to delete the current session?",
                ShowDateTime = false,
                ActionBeforeClose = isConfirmed =>
                {
                    if (isConfirmed)
                        DeleteSessionCommand.Execute(null);
                    return true;
                }
            });
        }

        private async void SessionBoxContext_SessionCreated()
        {
            await EvaluateConditions();
        }
        private async Task DeleteSessionActionCommand(object arg)
        {
            var deletedSession = _sessionStore.CurrentSession;
            await _sessionService.DeleteSession(_sessionStore.CurrentSession.UniqueId);
            await _sessionStore.UpdateSession();

            await _transactionStore.InitializeAsync();
            await _inventoryStore.InitializeAsync();

            await EvaluateConditions();

            Growl.Info($"Successfully delete session \"{deletedSession.Period:D}\".");
            Growl.Ask(new GrowlInfo
            {
                Message = "Would you like to create a new session?",
                ShowDateTime = false,
                ActionBeforeClose = isConfirmed =>
                {
                    if (isConfirmed)
                        SessionBoxContext.IsSessionBox = true;
                    return true;
                }
            });
        }

        private async Task SaveEditActionCommand(object obj)
        {
            ShowCalendar = false;

            var products = await _transactionService.GetLatests();
            _transactionStore.LoadTransactions(products);

            var oldSession = _sessionStore.CurrentSession.Period;

            _sessionStore.CurrentSession.Period = EditCalendarDate;
            var newSession = await _sessionService.EditSession(_sessionStore.CurrentSession.UniqueId, new Session
            {
                Period = EditCalendarDate,
                UniqueId = _sessionStore.CurrentSession.UniqueId
            });
            await _sessionStore.UpdateSession();

            Growl.Success($"Successfully updated session from \"{oldSession:D}\" to \"{_sessionStore.CurrentSession.Period:D}\".");
        }

        private void EditSessionActionCommand(object obj)
        {
            ShowCalendar = true;
        }

        private void NewButtonActionCommand(object obj)
        {
            SessionBoxContext.IsSessionBox = true;
        }

        public static async Task<StartupViewModel> LoadStartupViewModel(IServiceProvider sp)
        {
            var vm = ActivatorUtilities.CreateInstance<StartupViewModel>(sp);
            await vm.InitializeAsync();
            return vm;
        }

        private async Task InitializeAsync()
        {
            await EvaluateConditions();
        }

        private async Task EvaluateConditions()
        {
            var session = await _sessionStore.UpdateSession();

            HasTransactions = session?.Transactions?.Any() == true;
            CanCreateSession = _sessionStore.CurrentSession == null;
            CanStartSession = _sessionStore.CurrentSession != null;
        }
    }

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

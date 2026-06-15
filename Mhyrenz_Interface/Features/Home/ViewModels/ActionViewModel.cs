using System;
using System.Threading.Tasks;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Features.Home.Controls;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Home.ViewModels
{
    public class ActionViewModel : BaseViewModel
    {
        private readonly ISessionService _sessionService;
        private readonly ISessionStore _sessionStore;
        private readonly ITransactionStore _transactionStore;
        private readonly IInventoryStore _inventoryStore;
        private readonly IUndoRedoManager _undoRedoManager;
        private readonly ICheckoutService _checkoutService;
        private readonly CreateViewModel<SessionBoxContext> _sessionBoxContext;

        public RelayCommand RegisterCommand { get; }
        public RelayCommand SundryCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand CreateCommand { get; }
        public AsyncRelayCommand SaveEditCommand { get; }
        public AsyncRelayCommand DeleteCommand { get; }
        public RelayCommand StartSessionCommand { get; }

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

        private bool _showCalendar = false;
        public bool ShowCalendar
        {
            get => _showCalendar;
            set
            {
                if (_showCalendar == value)
                    return;

                _showCalendar = value;

                if (!_showCalendar && EditCalendarDate.Date != _sessionStore.CurrentSession.Period.Date)
                    SaveEditCommand.Execute(null);

                EditCalendarDate = _sessionStore.CurrentSession.Period;
                OnPropertyChanged(nameof(ShowCalendar));
            }
        }

        public ActionViewModel(ISessionService sessionService,
            ISessionStore sessionStore,
            ITransactionStore transactionStore,
            IInventoryStore inventoryStore,
            IUndoRedoManager undoRedoManager,
            ICheckoutService checkoutService,
            CreateViewModel<SessionBoxContext> sessionBoxContext)
        {
            _sessionService = sessionService;
            _sessionStore = sessionStore;
            _transactionStore = transactionStore;
            _inventoryStore = inventoryStore;
            _undoRedoManager = undoRedoManager;
            _checkoutService = checkoutService;
            _sessionBoxContext = sessionBoxContext;

            RegisterCommand = new RelayCommand(RegisterActionCommand, CanRegisterCommand);
            SundryCommand = new RelayCommand(SundryActionCommand, CanSundryCommand);
            CreateCommand = new RelayCommand(CreateActionCommand, CanCreateCommand);
            EditCommand = new RelayCommand(EditSessionActionCommand, CanEditCommand);
            SaveEditCommand = new AsyncRelayCommand(SaveEditActionCommand);
            DeleteCommand = new AsyncRelayCommand(DeleteActionCommand, CanDeleteCommand);
        }

        private void SundryActionCommand(object obj)
        {
            throw new NotImplementedException();
        }


        private void RegisterActionCommand(object obj)
        {
            throw new NotImplementedException();
        }
        private bool CanSundryCommand(object obj)
        {
            return _sessionStore.CurrentSession != null;
        }

        private bool CanRegisterCommand(object obj)
        {
            return _sessionStore.CurrentSession != null;
        }

        private bool CanDeleteCommand(object obj)
        {
            return _sessionStore.CurrentSession != null;
        }

        private bool CanCreateCommand(object obj)
        {
            return _sessionStore.CurrentSession == null;
        }

        private bool CanEditCommand(object obj)
        {
            return _sessionStore.CurrentSession != null;
        }

        private void CreateActionCommand(object obj)
        {
            CommandsOnCanExecutedChanged();

            Dialog dialog = Dialog.Show<SessionBox>();
            dialog.SetValue(Dialog.MaskCanCloseProperty, true);

            var vm = _sessionBoxContext();
            dialog.SetValue(Dialog.DataContextProperty, vm);
            dialog.Unloaded += dialog_Unloaded;

            void dialog_Unloaded(object sender, System.Windows.RoutedEventArgs e)
            {
                dialog.Unloaded -= dialog_Unloaded;
                dialog.GetViewModel<SessionBoxContext>().Dispose();
            }
        }

        private async Task DeleteActionCommand(object arg)
        {
            var deletedSession = _sessionStore.CurrentSession;
            var prompt = MessageBox.Show($"You're about to delete an existing session: {deletedSession.Period:D}, would you like to proceed?",
                "Delete Session",
                System.Windows.MessageBoxButton.YesNoCancel,
                System.Windows.MessageBoxImage.Question);

            if (_transactionStore.Store.Count > 0 && prompt == System.Windows.MessageBoxResult.Yes)
            {
                prompt = MessageBox.Show("Existing transactions has been found in the current session. If you wish to continue, the delete action cannot be reverted.",
                    "Delete Session - Transactions detected",
                    System.Windows.MessageBoxButton.OKCancel,
                    System.Windows.MessageBoxImage.Warning);
            }

            if (prompt == System.Windows.MessageBoxResult.Cancel || prompt == System.Windows.MessageBoxResult.No)
                return;

            _undoRedoManager.Clear();

            await _sessionService.DeleteSession(_sessionStore.CurrentSession.Id);

            await _sessionStore.UpdateSession();
            await _inventoryStore.InitializeAsync();
            await _transactionStore.InitializeAsync();

            CommandsOnCanExecutedChanged();

            Growl.Info($"Successfully delete session \"{deletedSession.Period:D}\".");
            Growl.Ask(new GrowlInfo
            {
                Message = "Would you like to create a new session?",
                ShowDateTime = false,
                ActionBeforeClose = isConfirmed =>
                {
                    if (!isConfirmed)
                        return false;

                    CreateCommand.Execute(null);
                    return true;
                }
            });
        }

        private void CommandsOnCanExecutedChanged()
        {
            RegisterCommand.OnCanExecuteChanged();
            SundryCommand.OnCanExecuteChanged();
            EditCommand.OnCanExecuteChanged();
            CreateCommand.OnCanExecuteChanged();
            DeleteCommand.OnCanExecuteChanged();
        }

        private async Task SaveEditActionCommand(object arg)
        {
            var date = EditCalendarDate;
            var prompt = MessageBox.Show($"Would you like to change the existing session's period from {_sessionStore.CurrentSession.Period:D} to {date:D}?",
                "Save Changes",
                System.Windows.MessageBoxButton.YesNoCancel,
                System.Windows.MessageBoxImage.Question);

            if (prompt == System.Windows.MessageBoxResult.Cancel || prompt == System.Windows.MessageBoxResult.No)
                return;

            ShowCalendar = false;

            var oldSession = _sessionStore.CurrentSession.Period;

            _sessionStore.CurrentSession.Period = date;
            var newSession = await _sessionService.EditSession(_sessionStore.CurrentSession.Id, date);
            await _sessionStore.UpdateSession();

            Growl.Success($"Successfully updated session from \"{oldSession:D}\" to \"{_sessionStore.CurrentSession.Period:D}\".");
        }

        private void EditSessionActionCommand(object obj)
        {
            ShowCalendar = true;
        }
    }
}

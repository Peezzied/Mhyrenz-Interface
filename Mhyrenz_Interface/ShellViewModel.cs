using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using HandyControl.Controls;
using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Mhyrenz_Interface.Features.Checkout.Views;
using Mhyrenz_Interface.Features.Home.Views;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Shared.Commands;
using Mhyrenz_Interface.Shared.Converters;
using Mhyrenz_Interface.Store;
using MenuItem = Mhyrenz_Interface.Shared.Controls.MenuItem;

namespace Mhyrenz_Interface
{
    public class ShellViewModel : BaseViewModel, IAsyncInitializable
    {
        private readonly INavigationServiceEx _navigationService;
        private readonly ITransactionStore _transactionStore;
        private readonly IInventoryStore _inventoryStore;
        private readonly IProductService _productService;
        private readonly IUndoRedoManager _undoRedoManger;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly DateTime _baseTime;
        private readonly ISessionStore _sessionStore;

        public ShellViewModel(
            ITransactionStore transactionStore,
            IInventoryStore inventoryStore,
            ISessionStore sessionStore,
            INavigationServiceEx navigationServiceEx,
            IDialogCoordinator dialogCoordinator,
            IUndoRedoManager undoRedoManager,
            ISerialBarcodeService serialBarcodeService)
        {

            serialBarcodeService.OnSerialConnected += SerialBarcodeService_OnSerialConnected;
            serialBarcodeService.OnSerialDisconnected += SerialBarcodeService_OnSerialDisconnected;
            serialBarcodeService.OnBarcodeReceived += SerialBarcodeService_OnBarcodeReceived;

            serialBarcodeService.Start("COM2");
            _transactionStore = transactionStore;
            _inventoryStore = inventoryStore;
            _sessionStore = sessionStore;
            _sessionStore.SessionChanged += SessionStore_SessionChanged;

            _navigationService = navigationServiceEx;
            _navigationService.Navigated += OnCurrentViewModelChanged;
            _undoRedoManger = undoRedoManager;

            UndoCommand = new AsyncRelayCommand(UndoRedoActionCommand, (parameter) => _undoRedoManger.CanUndo && !RedoCommand.IsExecuting);
            RedoCommand = new AsyncRelayCommand(UndoRedoActionCommand, (parameter) => _undoRedoManger.CanRedo && !UndoCommand.IsExecuting);

            _dialogCoordinator = dialogCoordinator;

            NavigateCommand = new AsyncRelayCommand(NavigateAction);

            _baseTime = DateTime.Now;
            _stopwatch.Start();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200) // smooth enough but light
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            _undoRedoManger.UndoRedoChanged += UndoRedoManger_UndoRedoChanged;
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            await NavigateToDefaultPageAsync();

            await _sessionStore.UpdateSession();

            var sales = _sessionStore.CurrentSession?.Sales.Where(s => s.Completed_at == null);
            if (sales?.Any() ?? false)
            {
                var salesSet = sales.Select(s => s.Id).ToHashSet();
                var productsInSales = _transactionStore.Store
                    .Where(t => t.Transaction.SaleId.HasValue && salesSet.Contains(t.Transaction.SaleId.Value))
                    .Select(t => t.Transaction.ProductId)
                    .ToHashSet();

                foreach (var product in _inventoryStore.Store)
                {
                    if (productsInSales.Contains(product.Item.Id))
                        product.HasActiveSale = true;
                }
            }
        }

        public ObservableCollection<MenuItem> Menu { get; } = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> OptionsMenu { get; } = new ObservableCollection<MenuItem>();
        public ICommand NavigateCommand { get; }
        public AsyncRelayCommand UndoCommand { get; set; }
        public AsyncRelayCommand RedoCommand { get; private set; }
        public bool CanMainBarcodeReceive { get; private set; } = true;


        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private MenuItem _selectedMenuItem;
        public MenuItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set => SetProperty(ref _selectedMenuItem, value);
        }

        private MenuItem _selectedOptionsMenuItem;
        public MenuItem SelectedOptionsMenuItem
        {
            get => _selectedOptionsMenuItem;
            set => SetProperty(ref _selectedOptionsMenuItem, value);
        }

        private object _ribbonBarViewModel;
        public object RibbonBarViewModel
        {
            get => _ribbonBarViewModel;
            set => SetProperty(ref _ribbonBarViewModel, value);
        }

        private bool _isReady;
        public bool IsReady
        {
            get => _isReady;
            set => SetProperty(ref _isReady, value);
        }


        private DateTime _today;
        public DateTime Today
        {
            get => _today;
            set => SetProperty(ref _today, value);
        }

        private int _seconds;
        public int Seconds
        {
            get => _seconds;
            set => SetProperty(ref _seconds, value);
        }

        public bool HasSession => SessionPeriod.HasValue;

        private DateTime? _sessionPeriod;
        public DateTime? SessionPeriod
        {
            get => _sessionPeriod;
            set
            {
                if (SetProperty(ref _sessionPeriod, value))
                    OnPropertyChanged(nameof(HasSession));
            }
        }


        public void SuspendMainBarcodeReceiver()
        {
            CanMainBarcodeReceive = false;
        }

        public void OpenMainBarcodeReceiver()
        {
            CanMainBarcodeReceive = true;
        }

        private async Task NavigateToDefaultPageAsync()
        {
            var defaultItem = Menu.FirstOrDefault(x => x.ViewType == typeof(HomeView))
                ?? Menu.FirstOrDefault();

            if (defaultItem == null)
                return;

            IsLoading = true;

            try
            {
                var navigated =
                    await _navigationService.NavigateAsync(
                        defaultItem.ViewType);

                if (navigated)
                    SelectedMenuItem = defaultItem;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnCurrentViewModelChanged(BaseViewModel vm, Type viewType)
        {
            CurrentViewModel = vm;

            if (CurrentViewModel != null)
                SelectedMenuItem = Menu.FirstOrDefault(x => x.ViewType == viewType);
        }

        private void UndoRedoManger_UndoRedoChanged(object sender, EventArgs e)
        {
            UndoCommand.OnCanExecuteChanged();
            RedoCommand.OnCanExecuteChanged();
        }

        private void SessionStore_SessionChanged(Session session)
        {
            SessionPeriod = session?.Period;
        }

        private void SerialBarcodeService_OnSerialDisconnected()
        {
            IsReady = false;
        }

        private void SerialBarcodeService_OnSerialConnected()
        {
            IsReady = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var accurateNow = _baseTime + _stopwatch.Elapsed;

            Today = accurateNow;
            Seconds = accurateNow.Second;
        }

        private void SerialBarcodeService_OnBarcodeReceived(string barcode)
        {
            App.Current.Dispatcher.Invoke(async () =>
            {
                if (CanMainBarcodeReceive)
                {
                    var win = App.Current.MainWindow;
                    if (win.Visibility == System.Windows.Visibility.Visible
                        && win.WindowState != WindowState.Minimized
                        && CurrentViewModel is CheckoutViewModel checkout)
                    {
                        if (!win.IsActive)
                            ToggleActivateMainWindow.ActivateMainWindow();

                        await checkout.ReceiveBarcode(barcode);
                        NotifyIcon.ShowBalloonTip("Barcode Scan", $"{barcode}", HandyControl.Data.NotifyIconInfoType.Info, MainWindow.AppTrayToken);
                    }
                    else
                    {
                        NotifyIcon.ShowBalloonTip("Missed Barcode", "Scan the barcode again in a correct checkout tab.", HandyControl.Data.NotifyIconInfoType.Warning, MainWindow.AppTrayToken);
                        ToggleActivateMainWindow.ActivateMainWindow();

                        await Navigate(Menu.FirstOrDefault(m => m.ViewType == typeof(CheckoutView)));
                    }
                }
            });
        }

        private async Task UndoRedoActionCommand(object parameter)
        {
            switch ((ActionType)parameter)
            {
                case ActionType.Undo:
                    await _undoRedoManger.Undo();
                    break;
                case ActionType.Redo:
                    await _undoRedoManger.Redo();
                    break;
            }
        }

        private async Task NavigateAction(object parameter)
        {
            //var isMainMenu = ReferenceEquals(parameters.MenuItem, Menu); TODO only applicable when there's an optionmenu

            if (!(parameter is MenuItem menuItem))
                return;
            await Navigate(menuItem);
        }

        private async Task Navigate(MenuItem menuItem, PostNavigation postNavigation = null)
        {
            IsLoading = true;

            var navigated = false;

            try
            {
                navigated = await _navigationService.NavigateAsync(menuItem.ViewType, postNavigation);
            }
            finally
            {
                if (navigated)
                {
                    SelectedMenuItem = menuItem;
                    SelectedOptionsMenuItem = null;

                }

                IsLoading = false;
            }
        }

        //public override void Dispose()
        //{
        //    _navigationService.CurrentViewModelChanged -= OnCurrentViewModelChanged;
        //    CurrentViewModel?.Dispose();
        //    base.Dispose();
        //}
    }
}

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Features.Checkout.Views;
using Mhyrenz_Interface.Features.Home.Views;
using Mhyrenz_Interface.Features.Inventory.Views;
using Mhyrenz_Interface.Features.Settings.Views;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Shared.Converters;
using Mhyrenz_Interface.Store;
using MenuItem = Mhyrenz_Interface.Shared.Controls.MenuItem;

namespace Mhyrenz_Interface
{
    public class ShellViewModel : BaseViewModel
    {
        private readonly INavigationServiceEx _navigationService;
        private readonly IInventoryStore _inventoryStore;
        private readonly IProductService _productService;
        private readonly IUndoRedoManager _undoRedoManger;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly DateTime _baseTime;


        public ShellViewModel(
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

            sessionStore.StateChanged += SessionStore_SessionChanged;
            Session = sessionStore.CurrentSession.Period.ToString("ddd MMM d, yyyy");

            _navigationService = navigationServiceEx;
            _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;
            _undoRedoManger = undoRedoManager;

            UndoCommand = new AsyncRelayCommand(UndoRedoActionCommand, (parameter) => _undoRedoManger.CanUndo);
            RedoCommand = new AsyncRelayCommand(UndoRedoActionCommand, (parameter) => _undoRedoManger.CanRedo);

            _dialogCoordinator = dialogCoordinator;

            NavigateCommand = new RelayCommand<NavigationCommandParams>(Navigate);


            // Build the menus
            Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.HouseSolid },
                Label = "Home",
                ViewType = typeof(HomeView)
            });
            Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.CashRegisterSolid },
                Label = "Checkout",
                ViewType = typeof(CheckoutView)
            });
            Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FolderSolid },
                Label = "Inventory",
                ViewType = typeof(InventoryView)
            });
            OptionsMenu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.GearSolid },
                Label = "Settings",
                ViewType = typeof(SettingsView)
            });

            _baseTime = DateTime.Now;
            _stopwatch.Start();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200) // smooth enough but light
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            _undoRedoManger.UndoRedoChanged += UndoRedoManger_UndoRedoChanged;

            App.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                await NavigateToDefaultPageAsync();
            }));
        }


        public ObservableCollection<MenuItem> Menu { get; } = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> OptionsMenu { get; } = new ObservableCollection<MenuItem>();
        public ICommand NavigateCommand { get; }
        public AsyncRelayCommand UndoCommand { get; set; }
        public AsyncRelayCommand RedoCommand { get; private set; }
        public bool CanMainBarcodeReceive { get; private set; } = true;


        private NavigationViewModel _currentViewModel;
        public NavigationViewModel CurrentViewModel
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

        private string _session;
        public string Session
        {
            get => _session;
            set => SetProperty(ref _session, value);
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

        private void OnCurrentViewModelChanged(NavigationViewModel vm)
        {
            CurrentViewModel = vm;
        }

        private void UndoRedoManger_UndoRedoChanged(object sender, EventArgs e)
        {
            UndoCommand.OnCanExecuteChanged();
            RedoCommand.OnCanExecuteChanged();
        }

        private void SessionStore_SessionChanged(Session obj)
        {
            Session = obj?.Period.ToString("ddd MMM d, yyyy");
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

        private void SerialBarcodeService_OnBarcodeReceived(string obj)
        {
            //throw new NotImplementedException();
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

        private async void Navigate(NavigationCommandParams parameters)
        {
            var isMainMenu = ReferenceEquals(parameters.MenuItem, Menu);

            var menuItem = isMainMenu
                ? parameters.Menu.SelectedItem as MenuItem
                : parameters.Menu.SelectedOptionsItem as MenuItem;

            if (menuItem == null)
                return;

            IsLoading = true;

            var navigated = false;

            try
            {
                navigated = await _navigationService.NavigateAsync(menuItem.ViewType);
            }
            finally
            {
                if (navigated)
                {
                    if (isMainMenu)
                    {
                        SelectedMenuItem = menuItem;
                        SelectedOptionsMenuItem = null;
                    }
                    else
                    {
                        SelectedOptionsMenuItem = menuItem;
                        SelectedMenuItem = null;
                    }
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

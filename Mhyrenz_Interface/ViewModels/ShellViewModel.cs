using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Mhyrenz_Interface.Converters;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels.Factory;
using Mhyrenz_Interface.Views;
using MenuItem = Mhyrenz_Interface.Controls.MenuItem;

namespace Mhyrenz_Interface.ViewModels
{
    public class ShellViewModel : BaseViewModel
    {
        private readonly INavigationServiceEx _navigationServiceEx;
        private readonly IViewModelFactory<NavigationViewModel> _viewModelFactory;

        private readonly ObservableCollection<MenuItem> AppMenu = new ObservableCollection<MenuItem>();
        private readonly ObservableCollection<MenuItem> AppOptionsMenu = new ObservableCollection<MenuItem>();
        private readonly IInventoryStore _inventoryStore;
        private readonly IProductService _productService;
        private readonly IUndoRedoManager _undoRedoManger;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private DateTime _baseTime;

        public BaseViewModel CurrentViewModel => _navigationServiceEx.CurrentViewModel;

        public ObservableCollection<MenuItem> Menu => AppMenu;

        public ObservableCollection<MenuItem> OptionsMenu => AppOptionsMenu;

        public ICommand NavigateCommand { get; }

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
            get
            {
                return _ribbonBarViewModel;
            }
            set
            {
                _ribbonBarViewModel = value;
                OnPropertyChanged(nameof(RibbonBarViewModel));
            }
        }

        private bool _isReady;
        public bool IsReady
        {
            get => _isReady;
            set
            {
                _isReady = value;
                OnPropertyChanged(nameof(IsReady));
            }
        }


        private DateTime _today;
        public DateTime Today
        {
            get => _today;
            set
            {
                _today = value;
                OnPropertyChanged(nameof(Today));
            }
        }

        private int _seconds;
        public int Seconds
        {
            get => _seconds;
            set
            {
                _seconds = value;
                OnPropertyChanged(nameof(Seconds));
            }
        }

        private string _session;
        public string Session
        {
            get => _session;
            set
            {
                _session = value;
                OnPropertyChanged(nameof(Session));
            }
        }

        public ICommand UndoCommand { get; set; }
        public ICommand RedoCommand { get; private set; }
        public bool CanMainBarcodeReceive { get; private set; } = true;

        public ShellViewModel(
            ISessionStore sessionStore,
            IInventoryStore inventroyStore,
            IProductService productService,
            INavigationServiceEx navigationServiceEx,
            NavigationViewModelFactory viewModelFactory,
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

            _navigationServiceEx = navigationServiceEx;
            _navigationServiceEx.Navigated += OnNavigated;
            _undoRedoManger = undoRedoManager;

            UndoCommand = new RelayCommand(UndoRedoActionCommand, (parameter) => _undoRedoManger.CanUndo);
            RedoCommand = new RelayCommand(UndoRedoActionCommand, (parameter) => _undoRedoManger.CanRedo);

            _dialogCoordinator = dialogCoordinator;

            NavigateCommand = new RelayCommand<NavigationCommandParams>(Navigate);

            _viewModelFactory = viewModelFactory;

            // Build the menus
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.HouseSolid },
                Label = "Home",
                NavigationType = typeof(HomeView)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.CashRegisterSolid },
                Label = "Checkout",
                NavigationType = typeof(CheckoutView)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FolderSolid },
                Label = "Inventory",
                NavigationType = typeof(InventoryView)
            });
            this.OptionsMenu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.GearSolid },
                Label = "Settings",
                NavigationType = typeof(SettingsView)
            });

            _navigationServiceEx.Navigate(typeof(HomeView));
            _inventoryStore = inventroyStore;
            _productService = productService;

            _baseTime = DateTime.Now;
            _stopwatch.Start();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200) // smooth enough but light
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
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

        private void UndoRedoActionCommand(object parameter)
        {
            switch (parameter.CastTo<ActionType>())
            {
                case ActionType.Undo:
                    _undoRedoManger.Undo();
                    break;
                case ActionType.Redo:
                    _undoRedoManger.Redo();
                    break;
            }
        }

        public void OnTransitionComplete()
        {
            _navigationServiceEx.TransitionComplete();
        }

        private void Navigate(NavigationCommandParams parameters)
        {
            var selectedItem = ReferenceEquals(parameters.MenuItem, Menu)
                ? parameters.Menu.SelectedItem
                : parameters.Menu.SelectedOptionsItem;

            if (selectedItem is MenuItem menuItem && menuItem.NavigationType != null)
            {
                _navigationServiceEx.Navigate(menuItem.NavigationType);
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            var contentType = e.Content?.GetType();
            var lastOptionsMenuItem = SelectedOptionsMenuItem;

            //Debug.WriteLine($"Navigated to: {contentType?.Name}");

            SelectedMenuItem = Menu.FirstOrDefault(x => x.NavigationType == contentType);
            SelectedOptionsMenuItem = OptionsMenu.FirstOrDefault(x => x.NavigationType == contentType);

            UpdateCurrentViewModel(contentType);


        }

        private void UpdateCurrentViewModel(Type viewType)
        {
            var vm = _viewModelFactory.CreateViewModel(viewType);
            _navigationServiceEx.CurrentViewModel = vm;
            OnPropertyChanged(nameof(CurrentViewModel));

            RibbonBarViewModel = this;
        }

        internal void SuspendMainBarcodeReceiver()
        {
            CanMainBarcodeReceive = false;
        }

        internal void OpenMainBarcodeReceiver()
        {
            CanMainBarcodeReceive = true;
        }
    }
}

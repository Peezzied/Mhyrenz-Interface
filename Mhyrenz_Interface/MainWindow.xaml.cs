using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using HandyControl.Controls;
using HandyControl.Tools;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using Mhyrenz_Interface.Features.Checkout.Views;
using Mhyrenz_Interface.Features.Home.Views;
using Mhyrenz_Interface.Features.Inventory.Views;
using Mhyrenz_Interface.Shared.Controls;
using Mhyrenz_Interface.Store;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : MetroWindow
    {
        private readonly IUndoRedoManager _undoRedoManager;
        private bool _isFullscreen;
        private WindowState _prevState;
        private WindowStyle _prevStyle;
        private ResizeMode _prevResizeMode;
        private Rect _prevBounds;
        private bool _hasNotifyClose = false;

        public static string AppTrayToken { get; } = "AppTray";

        public MainWindow(ShellViewModel shellVIewModel, IUndoRedoManager undoRedoManager)
        {
            DataContext = shellVIewModel;
            _undoRedoManager = undoRedoManager;

            Closing += MainWindow_Closing;

            var menu = shellVIewModel.Menu;

            menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.HouseSolid },
                Label = "Home",
                ViewType = typeof(HomeView)
            });
            var checkout = new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.CashRegisterSolid },
                Label = "Checkout",
                ViewType = typeof(CheckoutView)
            };
            BindingOperations.SetBinding(checkout, MenuItem.IsEnabledProperty, new Binding(nameof(ShellViewModel.HasSession))
            {
                Source = shellVIewModel,
                Mode = BindingMode.OneWay
            });
            menu.Add(checkout);
            menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FolderSolid },
                Label = "Inventory",
                ViewType = typeof(InventoryView)
            });

            NotifyIcon.Register(AppTrayToken, AppTray);

            InitializeComponent();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_undoRedoManager.CanUndo || _undoRedoManager.CanRedo)
            {
                var prompt = MessageBox.Show("Are you sure you want to exit after the changes you've made?",
                    "Inventory Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (prompt == MessageBoxResult.Cancel || prompt == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                Closing -= MainWindow_Closing;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();

            if (!_hasNotifyClose)
            {
                AppTray.ShowBalloonTip(
                    "System tray",
                    "The application is still running.", HandyControl.Data.NotifyIconInfoType.Info);
                _hasNotifyClose = true;
            }

            e.Cancel = true;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                ToggleFullScreen_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }

        private void ToggleFullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (!_isFullscreen)
            {
                _prevState = WindowState;
                _prevStyle = WindowStyle;
                _prevResizeMode = ResizeMode;
                _prevBounds = new Rect(Left, Top, Width, Height);

                // Restore first so Width/Height can be set.
                WindowState = WindowState.Normal;

                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;

                var screen = System.Windows.Forms.Screen.FromHandle(
                    new System.Windows.Interop.WindowInteropHelper(this).Handle);

                var bounds = screen.Bounds;

                Left = bounds.Left;
                Top = bounds.Top;
                Width = bounds.Width;
                Height = bounds.Height;

                _isFullscreen = true;
            }
            else
            {
                WindowStyle = _prevStyle;
                ResizeMode = _prevResizeMode;

                Left = _prevBounds.Left;
                Top = _prevBounds.Top;
                Width = _prevBounds.Width;
                Height = _prevBounds.Height;
                WindowState = _prevState;

                _isFullscreen = false;
            }
        }
    }
}

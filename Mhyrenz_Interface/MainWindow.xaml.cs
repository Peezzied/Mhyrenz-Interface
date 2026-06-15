using System.Windows;
using System.Windows.Data;
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
    public partial class MainWindow : MetroWindow
    {
        private readonly IUndoRedoManager _undoRedoManager;

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

            InitializeComponent();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_undoRedoManager.CanUndo)
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
    }
}

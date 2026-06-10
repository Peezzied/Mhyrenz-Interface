using System.Windows;
using MahApps.Metro.Controls;
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

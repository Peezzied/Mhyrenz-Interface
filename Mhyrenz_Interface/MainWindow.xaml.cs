using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Mhyrenz_Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly INavigationServiceEx _navigationServiceEx;
        private readonly IUndoRedoManager _undoRedoManager;

        public Frame NavigationFrame => _navigationServiceEx.Frame;

        public MainWindow(BaseViewModel dataContext, INavigationServiceEx navigationServiceEx, IUndoRedoManager undoRedoManager)
        {
            DataContext = dataContext;
            _navigationServiceEx = navigationServiceEx;
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
            }
        }

        private void TransitioningContentControl_TransitionCompleted(object sender, RoutedEventArgs e)
        {
            ((ShellViewModel)DataContext).OnTransitionComplete();
        }
    }
}

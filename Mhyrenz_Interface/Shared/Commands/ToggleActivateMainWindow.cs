using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HandyControl.Tools;

namespace Mhyrenz_Interface.Shared.Commands
{
    public class ToggleActivateMainWindow : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            var win = App.Current.MainWindow;
            if (win != null)
            {
                if (win.Visibility != Visibility.Visible || parameter is bool)
                {
                    ActivateMainWindow();
                }
                else
                {
                    App.Current.MainWindow.Hide();
                }
            }
        }

        public static void ActivateMainWindow()
        {
            var win = App.Current.MainWindow;
            win.Activate();
            win.Show();
            win.Focus();
            WindowHelper.SetWindowToForeground(win);
        }
    }
}

using System.Windows;
using Mhyrenz_Interface.Core.MVVM;

namespace Mhyrenz_Interface.Startup
{
    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        public StartupWindow(BaseViewModel viewModel)
        {
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}

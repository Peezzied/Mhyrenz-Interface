using System.Windows;

namespace Mhyrenz_Interface.Views
{
    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    public partial class Startup : Window
    {
        public Startup(BaseViewModel viewModel)
        {
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}

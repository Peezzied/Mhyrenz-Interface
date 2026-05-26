using System;
using System.Windows.Controls;

namespace Mhyrenz_Interface.Views
{

    public partial class CheckoutView : UserControl
    {
        public CheckoutView()
        {
            InitializeComponent();
        }

        private void TabablzControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (TabControl.SelectedItem is IDisposable item)
            {
                item.Dispose();
            }
        }
    }
}

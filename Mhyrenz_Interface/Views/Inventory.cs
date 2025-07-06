using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using HandyControl.Themes;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.Views
{
    public partial class InventoryView : UserControl
    {
        private ContentControl DataGridPresenter;

        public InventoryView()
        {
            InitializeComponent(); 
        }

        private void ContentControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataGridPresenter = sender.CastTo<ContentControl>();
            DataContext.CastTo<InventoryViewModel>().AddItem += InventoryView_AddItem;
        }

        private async void InventoryView_AddItem(object sender, int index)
        {
            
        }

        private void ContentControl_Unloaded(object sender, RoutedEventArgs e)
        {
            DataContext.CastTo<InventoryViewModel>().AddItem -= InventoryView_AddItem;
        }

    }
}

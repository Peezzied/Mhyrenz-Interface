using Mhyrenz_Interface.Controls.Behaviors;
using Mhyrenz_Interface.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Mhyrenz_Interface.Controls
{
    public enum LayoutType
    {
        Compact,
        Detailed
    }
    public class InventoryDataGrid: DataGrid
    {
       
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Unloaded += InventoryDataGrid_Unloaded;
        }

        private void InventoryDataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            IEnumerable source = ((DataGrid)sender).ItemsSource;
            var view = (IEditableCollectionView)CollectionViewSource.GetDefaultView(source);
            view?.CommitEdit();
        }

        public LayoutType LayoutType
        {
            get { return (LayoutType)GetValue(LayoutTypeProperty); }
            set { SetValue(LayoutTypeProperty, value); }
        }

        public static readonly DependencyProperty LayoutTypeProperty =
            DependencyProperty.Register(nameof(LayoutType), typeof(LayoutType), typeof(InventoryDataGrid), new PropertyMetadata(null));

    }
}

using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mhyrenz_Interface.Controls.Behaviors
{
    public class BarcodeCellBehavior: Behavior<TextBox>
    {
        private IBarcodeBound _viewModel;

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void BarcodeCellBehavior_BarcodeReceived()
        {

        }

        private void AssociatedObject_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.CastTo<BaseViewModel>().Dispose();

            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AssociatedObject.DataContext.CastTo<IBarcodeBound>().BarcodeReceived += BarcodeCellBehavior_BarcodeReceived;
            _viewModel = AssociatedObject.DataContext.CastTo<IBarcodeBound>();

            _viewModel.Load();
        }
    }
}

using System.Windows.Controls;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xaml.Behaviors;

namespace Mhyrenz_Interface.Features.Inventory.Behaviors
{
    public class BarcodeCellBehavior : Behavior<TextBox>
    {
        private IBarcodeBound _viewModel;
        private readonly ShellViewModel MainViewModel = App.ServiceProvider.GetRequiredService<ShellViewModel>();

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
            MainViewModel.OpenMainBarcodeReceiver();
            _viewModel.CastTo<BaseViewModel>().Dispose();

            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            MainViewModel.SuspendMainBarcodeReceiver();

            AssociatedObject.DataContext.CastTo<IBarcodeBound>().BarcodeReceived += BarcodeCellBehavior_BarcodeReceived;
            _viewModel = AssociatedObject.DataContext.CastTo<IBarcodeBound>();

            _viewModel.LoadReceiver();
        }
    }
}

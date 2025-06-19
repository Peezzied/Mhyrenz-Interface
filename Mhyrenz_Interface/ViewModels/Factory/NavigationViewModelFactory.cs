using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Views;
using Microsoft.Xaml.Behaviors.Core;
using System;
using System.Windows.Navigation;

namespace Mhyrenz_Interface.ViewModels.Factory
{
    public class NavigationViewModel: BaseViewModel 
    {
        private readonly INavigationServiceEx _navigationServiceEx;

        public NavigationViewModel(INavigationServiceEx navigationServiceEx)
        {
            _navigationServiceEx = navigationServiceEx;
            _navigationServiceEx.Navigating += (s, e) => Navigating?.Invoke(s, e);
            _navigationServiceEx.TransitionCompleted += () => TransitionCompleted?.Invoke();
        }

        public event EventHandler Navigating;
        public event Action TransitionCompleted;

        public override void Dispose()
        {
            _navigationServiceEx.Navigating -= (s, e) => Navigating?.Invoke(s, e);
            _navigationServiceEx.TransitionCompleted -= () => TransitionCompleted?.Invoke(); base.Dispose();
        }
    }
    public class NavigationViewModelFactory : IViewModelFactory<NavigationViewModel>
    {
        private readonly CreateViewModel<HomeViewModel> _createHomeViewModel;
        private readonly CreateViewModel<InventoryViewModel> _createInventoryViewModel;
        private readonly CreateViewModel<TransactionViewModel> _createTransactionsViewModel;
        private readonly CreateViewModel<SettingsViewModel> _createSettingsViewModel;

        public NavigationViewModelFactory(CreateViewModel<HomeViewModel> createHomeViewModel,
            CreateViewModel<InventoryViewModel> createInventoryViewModel,
            CreateViewModel<TransactionViewModel> createTransactionsViewModel,
            CreateViewModel<SettingsViewModel> createSettingsViewModel)
        {
            _createHomeViewModel = createHomeViewModel;
            _createInventoryViewModel = createInventoryViewModel;
            _createTransactionsViewModel = createTransactionsViewModel;
            _createSettingsViewModel = createSettingsViewModel;
        }

        public NavigationViewModel CreateViewModel(object parameter)
        {
            var viewType = parameter as Type;
            switch (viewType.Name)
            {
                case nameof(HomeView):
                    return _createHomeViewModel();
                case nameof(InventoryView):
                    return _createInventoryViewModel();
                case nameof(TransactionsView):
                    return _createTransactionsViewModel();
                case nameof(SettingsView):
                    return _createSettingsViewModel();
                default:
                    throw new ArgumentException($"No view model found for type {viewType.Name}");
            }
        }
    }
}

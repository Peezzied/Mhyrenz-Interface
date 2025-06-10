using Mhyrenz_Interface.Views;
using System;

namespace Mhyrenz_Interface.ViewModels.Factory
{
    public class NavigationViewModel: BaseViewModel { }
    public class NavigationViewModelFactory : IViewModelFactory<NavigationViewModel>
    {
        private readonly CreateViewModel<HomeViewModel> _createHomeViewModel;
        private readonly CreateViewModel<InventoryViewModel> _createInventoryViewModel;
        private readonly CreateViewModel<TransactionsViewModel> _createTransactionsViewModel;
        private readonly CreateViewModel<SettingsViewModel> _createSettingsViewModel;

        public NavigationViewModelFactory(CreateViewModel<HomeViewModel> createHomeViewModel,
            CreateViewModel<InventoryViewModel> createInventoryViewModel,
            CreateViewModel<TransactionsViewModel> createTransactionsViewModel,
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.ViewModels.Factory
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly CreateViewModel<HomeViewModel> _createHomeViewModel;
        private readonly CreateViewModel<InventoryViewModel> _createInventoryViewModel;
        private readonly CreateViewModel<TransactionsViewModel> _createTransactionsViewModel;
        private readonly CreateViewModel<SettingsViewModel> _createSettingsViewModel;

        public ViewModelFactory(CreateViewModel<HomeViewModel> createHomeViewModel,
            CreateViewModel<InventoryViewModel> createInventoryViewModel,
            CreateViewModel<TransactionsViewModel> createTransactionsViewModel,
            CreateViewModel<SettingsViewModel> createSettingsViewModel)
        {
            _createHomeViewModel = createHomeViewModel;
            _createInventoryViewModel = createInventoryViewModel;
            _createTransactionsViewModel = createTransactionsViewModel;
            _createSettingsViewModel = createSettingsViewModel;
        }

        public BaseViewModel CreateViewModel(Type viewType)
        {
            switch (viewType.Name)
            {
                case nameof(HomeViewModel):
                    return _createHomeViewModel();
                case nameof(InventoryViewModel):
                    return _createInventoryViewModel();
                case nameof(TransactionsViewModel):
                    return _createTransactionsViewModel();
                case nameof(SettingsViewModel):
                    return _createSettingsViewModel();
                default:
                    throw new ArgumentException($"No view model found for type {viewType.Name}");
            }
        }
    }
}

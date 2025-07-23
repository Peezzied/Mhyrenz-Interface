using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Views;
using Microsoft.Xaml.Behaviors.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<Type, (Type viewModelType, Delegate factory)> _viewsSet = new Dictionary<Type, (Type, Delegate)>();

        public NavigationViewModelFactory(CreateViewModel<HomeViewModel> createHomeViewModel,
            CreateViewModel<InventoryViewModel> createInventoryViewModel,
            CreateViewModel<TransactionViewModel> createTransactionsViewModel,
            CreateViewModel<SettingsViewModel> createSettingsViewModel)
        {
            _createHomeViewModel = createHomeViewModel;
            _createInventoryViewModel = createInventoryViewModel;
            _createTransactionsViewModel = createTransactionsViewModel;
            _createSettingsViewModel = createSettingsViewModel;

            _viewsSet[typeof(HomeView)] = (typeof(HomeViewModel), _createHomeViewModel);
            _viewsSet[typeof(InventoryView)] = (typeof(InventoryViewModel), _createInventoryViewModel);
            _viewsSet[typeof(TransactionsView)] = (typeof(TransactionViewModel), _createTransactionsViewModel);
            _viewsSet[typeof(SettingsView)] = (typeof(SettingsViewModel), _createSettingsViewModel);
        }

        public NavigationViewModel CreateViewModel(object parameter)
        {
            var viewType = parameter as Type;
            if (_viewsSet.TryGetValue(viewType, out var viewModel))
            {
                
                return viewModel.factory.CastTo<CreateViewModel<NavigationViewModel>>().Invoke();
            }

            throw new ArgumentException($"No view model found for type {viewType.Name}");
        }

        public Type GetViewByViewModel(NavigationViewModel viewModel)
        {
            var derivedSetToViewModels = _viewsSet.ToDictionary(v => v.Value.viewModelType, v => v.Key);
            var viewModelType = viewModel.GetType();

            if (derivedSetToViewModels.TryGetValue(viewModelType, out var viewType))
            {
                return viewType;
            }

            throw new ArgumentException($"No view found for type {viewModelType.Name}");
        }
    }
}

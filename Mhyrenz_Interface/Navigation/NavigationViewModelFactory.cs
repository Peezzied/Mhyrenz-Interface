using System;
using System.Collections.Generic;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.Utilities;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Mhyrenz_Interface.Features.Checkout.Views;
using Mhyrenz_Interface.Features.Home.ViewModels;
using Mhyrenz_Interface.Features.Home.Views;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Features.Inventory.Views;

namespace Mhyrenz_Interface.Navigation
{
    public class NavigationViewModelFactory : IViewModelFactory<BaseViewModel>
    {
        private readonly CreateViewModel<HomeViewModel> _createHomeViewModel;
        private readonly CreateViewModel<InventoryViewModel> _createInventoryViewModel;
        private readonly CreateViewModel<CheckoutViewModel> _createTransactionsViewModel;
        private static readonly Dictionary<Type, (Type viewModelType, Delegate factory)> _viewsSet = new Dictionary<Type, (Type, Delegate)>();

        public NavigationViewModelFactory(CreateViewModel<HomeViewModel> createHomeViewModel,
            CreateViewModel<InventoryViewModel> createInventoryViewModel,
            CreateViewModel<CheckoutViewModel> createTransactionsViewModel)
        {
            _createHomeViewModel = createHomeViewModel;
            _createInventoryViewModel = createInventoryViewModel;
            _createTransactionsViewModel = createTransactionsViewModel;

            _viewsSet[typeof(HomeView)] = (typeof(HomeViewModel), _createHomeViewModel);
            _viewsSet[typeof(InventoryView)] = (typeof(InventoryViewModel), _createInventoryViewModel);
            _viewsSet[typeof(CheckoutView)] = (typeof(CheckoutViewModel), _createTransactionsViewModel);
        }

        public BaseViewModel CreateViewModel(object parameter)
        {
            var viewType = parameter as Type;
            if (_viewsSet.TryGetValue(viewType, out var viewModel))
            {

                return viewModel.factory.CastTo<CreateViewModel<BaseViewModel>>().Invoke();
            }

            throw new ArgumentException($"No view model found for type {viewType.Name}");
        }

        public static Type GetViewModelType(Type viewType)
        {
            _viewsSet.TryGetValue(viewType, out var viewModelType);

            return viewModelType.viewModelType;
        }
    }
}

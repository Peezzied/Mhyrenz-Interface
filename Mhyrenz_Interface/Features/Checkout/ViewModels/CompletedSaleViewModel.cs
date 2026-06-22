using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Checkout.ViewModels
{
    public class CompletedSaleViewModel : BaseViewModel, IFlyoutViewModel
    {
        private readonly ISessionStore _sessionStore;

        public CompletedSaleViewModel(ICheckoutService checkoutService, ISessionStore sessionStore)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                CompletedSales.AddRange(await checkoutService.GetSalesHistory());
            }));

            CompletedSales.CollectionChanged += CompletedSales_CollectionChanged;
            _sessionStore = sessionStore;

            _sessionStore.SessionChanged += SessionStore_SessionChanged;
        }

        private void SessionStore_SessionChanged(Session obj)
        {
            if (obj == null)
            {
                CompletedSales.Clear();
                OnPropertyChanged(nameof(HasCompletedSales));
            }
        }

        private void CompletedSales_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasCompletedSales));
        }

        public ObservableCollection<Sale> CompletedSales { get; }
            = new ObservableCollection<Sale>();

        public bool HasCompletedSales => CompletedSales.Count > 0;

        public string FlyoutTitle => "Today's Sales History";

        public override void Dispose()
        {
            CompletedSales.CollectionChanged -= CompletedSales_CollectionChanged;
            _sessionStore.SessionChanged -= SessionStore_SessionChanged;
        }
    }
}

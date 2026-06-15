using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;

namespace Mhyrenz_Interface.Features.Checkout.ViewModels
{
    public class CompletedSaleViewModel : FlyoutViewModel
    {
        public CompletedSaleViewModel(ICheckoutService checkoutService): base(title: "Today's Sales History")
        {
            App.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                CompletedSales.AddRange(await checkoutService.GetHistory());
            }));

            CompletedSales.CollectionChanged += CompletedSales_CollectionChanged;
        }

        private void CompletedSales_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasCompletedSales));
        }

        public ObservableCollection<Sale> CompletedSales { get; }
            = new ObservableCollection<Sale>();

        public bool HasCompletedSales => CompletedSales.Count > 0;

        public override void Dispose()
        {
            CompletedSales.CollectionChanged -= CompletedSales_CollectionChanged;
        }
    }
}

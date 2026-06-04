using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;

namespace Mhyrenz_Interface.ViewModels
{
    public class CompletedSaleViewModel: BaseViewModel
    {
        public CompletedSaleViewModel(ICheckoutService checkoutService)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                CompletedSales.AddRange(await checkoutService.GetHistory());
            }));
        }

        public ObservableCollection<Sale> CompletedSales { get; }
            = new ObservableCollection<Sale>();
    }
}

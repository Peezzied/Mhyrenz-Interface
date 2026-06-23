using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core.PropertyTracking;
using Mhyrenz_Interface.Core.UndoRedo;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Models.Settings;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.TransactionService;
using Mhyrenz_Interface.Features.Checkout.Views;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Features.Orders.Commands;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Store;
using Microsoft.Extensions.Options;

namespace Mhyrenz_Interface.Features.Checkout.Commands
{
    public class TransactionVMCommandDiscount : UndoRedoBoundCommand, ISaleBoundCommand
    {
        private readonly DTO _dto;
        private readonly ICheckoutService _checkoutService;
        private readonly ITransactionStore _transactionStore;
        private readonly IOptions<DiscountSettings> _discountSettings;
        private DiscountResult _result;
        private TransactionVMRowInfo _rowInfo;

        public int SaleId { get; }

        public TransactionVMCommandDiscount(DTO dto, ICheckoutService checkoutService, ITransactionStore transactionStore, IOptions<DiscountSettings> discountSettings) : base(typeof(CheckoutView))
        {
            _dto = dto;
            _checkoutService = checkoutService;
            _transactionStore = transactionStore;
            _discountSettings = discountSettings;
            Completer = CompleterHandler;

            SaleId = _dto.SaleId;
        }

        private async Task CompleterHandler(NavigationViewModel navigationViewModel)
        {
            if (navigationViewModel is IDataGridTabHost host)
            {
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    host.RowIntoView(_rowInfo.Sale, null);
                });
                await Complete();
            }
        }

        public override async Task Command()
        {
            var discountSettings = _discountSettings.Value;

            _result = await _checkoutService.ApplyDiscount(new DiscountInfo
            {
                Discount = _dto.Discount,
                DiscountRate = discountSettings.GetRate(_dto.Discount)
            }, saleId: _dto.SaleId, transactions: _dto.Transactions, isReversed: Intent == ActionType.Undo);

            _rowInfo = new TransactionVMRowInfo
            {
                Sale = _dto.SaleId
            };

            if (Intent == ActionType.Normal)
            {
                await Complete();
            }
        }

        private async Task Complete()
        {
            _transactionStore.OnSaleChange(_result.Sale);

            // FIXME consider better solution because this is slow
            await Task.WhenAll(_result.Transactions.Select(t => _transactionStore.UpdateTransaction(t)));
        }

        public class DTO
        {
            public Discount Discount { get; set; }
            public int SaleId { get; internal set; }
            public IEnumerable<Transaction> Transactions { get; internal set; }
        }
    }
}

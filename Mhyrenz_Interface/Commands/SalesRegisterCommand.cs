using System;
using System.Linq;
using System.Threading.Tasks;
using HandyControl.Controls;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.Commands
{
    public class SalesRegisterCommand : BaseAsyncCommand
    {
        private readonly ISalesRecordService _salesRecordService;
        private readonly ITransactionStore _transactionStore;
        private readonly ITransactionsService _transactionService;
        private readonly ISessionStore _sessionStore;
        private readonly IInventoryStore _inventoryStore;
        private readonly ISalesRegisterHost _salesRegisterHost;

        public SalesRegisterCommand(
            ISalesRegisterHost salesRegisterHost,
            ISalesRecordService salesRecordService,
            ITransactionStore transactionStore,
            ITransactionsService transactionsService,
            ISessionStore sessionStore,
            IInventoryStore inventoryStore)
        {
            _salesRecordService = salesRecordService;
            _transactionStore = transactionStore;
            _transactionService = transactionsService;
            _sessionStore = sessionStore;
            _inventoryStore = inventoryStore;
            _salesRegisterHost = salesRegisterHost;
        }

        public event Action Error;

        public override async Task ExecuteAsync(object parameter)
        {
            var prompt = MessageBox.Show("You're about to register, would you like to proceed?",
                "Register Sales",
                System.Windows.MessageBoxButton.YesNoCancel,
                System.Windows.MessageBoxImage.Question);

            if (prompt == System.Windows.MessageBoxResult.Cancel || prompt == System.Windows.MessageBoxResult.No)
                return;

            _salesRegisterHost.IsRegistering = true;

            var transactions = _transactionStore.Transactions.Where(t => t.DTO.Session.Id.Equals(_sessionStore.CurrentSession.Id));

            if (!transactions.Any())
            {
                ShowErrorAndStopLoading("Unable to register without transactions. Please try again later.", showDateTime: false);
                return;
            }

            if (_sessionStore.CurrentSession is null)
            {
                ShowErrorAndStopLoading("Unable to register right now. Please create a session and try again later.", showDateTime: true);
                return;
            }

            var session = _sessionStore.CurrentSession;

            var sales = new SalesRecord
            {
                Sales = transactions.Select((t) => new Sale
                {
                    ProductId = t.Product.Item.Id,
                    Barcode = t.Barcode,
                    Price = t.Price,
                    Qty = t.Amount,
                    ProductName = t.Name,
                }),
                SessionId = session.Id,
                TotalPurchase = transactions.Sum(t => t.Amount),
                TotalSales = (double)transactions.Sum(t => t.DTO.Product.Item.NetRetail),
                Profit = (double)transactions.Sum(t => t.DTO.Product.Item.Profit),
                RegisteredAt = session.Period
            };

            var grouped = transactions.GroupBy(t => t.Product.Item.Id)
                .Select(g => new Product()
                {
                    Id = g.First().Product.Item.Id,
                    Qty = g.First().Product.NetQty
                }).ToList();


            _transactionStore.Transactions.Clear();
            await _transactionService.Clear();

            await _inventoryStore.Register(grouped);

            _sessionStore.CurrentSession = null;
            await _salesRecordService.RegisterSales(sales);

            _salesRegisterHost.IsRegistering = false;

            MessageBox.Show($"You have sucessfully registered {sales.TotalSales:C} of Sales and {sales.Profit:C} of Profit.",
                "Register Success",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            Growl.Success("Sales has been registered.");
        }

        private void ShowErrorAndStopLoading(string message, bool showDateTime)
        {
            Growl.Error(new HandyControl.Data.GrowlInfo
            {
                Message = message,
                ShowDateTime = showDateTime,
            });

            Error?.Invoke();

            _salesRegisterHost.IsRegistering = false;
        }
    }
}

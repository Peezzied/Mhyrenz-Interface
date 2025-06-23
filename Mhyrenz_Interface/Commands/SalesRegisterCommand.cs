using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mhyrenz_Interface.Commands
{
    public class SalesRegisterCommand : BaseAsyncCommand
    {
        private readonly ISalesRecordService _salesRecordService;
        private readonly ITransactionStore _transactionStore;
        private readonly ITransactionsService _transactionService;
        private readonly ISessionStore _sessionStore;
        private readonly IInventoryStore _inventoryStore;
        private readonly HomeViewModel _homeViewModel;

        public SalesRegisterCommand(
            HomeViewModel homeViewModel,
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
            _homeViewModel = homeViewModel;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            var prompt = MessageBox.Show("Do you really want to register?", "Register", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (prompt == MessageBoxResult.No)
                return;

            _homeViewModel.IsLoading = true;

            var transactions = _transactionStore.Transactions.Where(t => t.DTO.Session.UniqueId.Equals(_sessionStore.CurrentSession.UniqueId));

            var session =  _sessionStore.CurrentSession;

            var sales = new SalesRecord()
            {
                SessionId = session.UniqueId,
                TotalPurchase = transactions.Sum(t => t.Amount),
                TotalSales = (double)transactions.Sum(t => t.DTO.Product.Item.NetRetail), // REFACTOR TRANSACTION MODELTYPES decimal TO double
                Profit = (double)transactions.Sum(t => t.DTO.Product.Item.Profit), // REFACTOR TRANSACTION MODELTYPES decimal TO double
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

            _homeViewModel.IsLoading = false;

            MessageBox.Show("Register Success");
        }
    }
}

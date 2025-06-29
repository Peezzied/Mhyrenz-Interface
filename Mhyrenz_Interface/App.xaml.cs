﻿using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Domain.Services.TransactionService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Domain.State.Mediator;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Mhyrenz_Interface
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IServiceProvider serviceProvider = CreateServiceProvider();

            InventoryDbContextFactory contextFactory = serviceProvider.GetRequiredService<InventoryDbContextFactory>();
            using(InventoryDbContext context = contextFactory.CreateDbContext())
            {
                context.Database.Migrate();
            }
            
            MainWindow mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            var sessionStore = serviceProvider.GetRequiredService<ISessionStore>();
            var sessionService = serviceProvider.GetRequiredService<ISessionService>();

            var session = sessionService.GetSession().GetAwaiter().GetResult();

            if (session != null)
            {
                sessionStore.CurrentSession = session;
            }
            else
            {
                sessionStore.CurrentSession = sessionService.GenerateSession(new Session { Period = DateTime.Now }).GetAwaiter().GetResult();
            }

                base.OnStartup(e);
        }

        private IServiceProvider CreateServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            Action<DbContextOptionsBuilder> inventoryConfig = options =>
            {
                options.UseSqlite("Data Source=dev_inventory.db");
            };

            services
                .AddDbContext<InventoryDbContext>(inventoryConfig)
                .AddSingleton<InventoryDbContextFactory>(new InventoryDbContextFactory(inventoryConfig))

                .AddSingleton<UndoRedoManager>()
                .AddSingleton<ISessionStore, SessionStore>()
                .AddSingleton<IInventoryStore, InventoryStore>()
                .AddSingleton<ITransactionStore, TransactionStore>()
                .AddSingleton<ICategoryStore, CategoryStore>(s =>
                {
                    return CategoryStore.LoadCategoryStore(
                        s.GetRequiredService<ICategoryService>(),
                        s.GetRequiredService<IInventoryStore>()
                    );
                })

                .AddSingleton<IDialogCoordinator, DialogCoordinator>() // MahApps DIALOG

                .AddSingleton<INavigationServiceEx, NavigationServiceEx>()
                .AddSingleton<IViewModelFactory<NavigationViewModel>, NavigationViewModelFactory>()

                .AddSingleton<IViewModelFactory<ProductDataViewModel>, ViewModelFactory<ProductDataViewModel>>()
                .AddSingleton<IViewModelFactory<TransactionDataViewModel>, ViewModelFactory<TransactionDataViewModel>>()

                .AddSingleton<IViewModelFactory<InventoryDataGridViewModel>, ViewModelFactory<InventoryDataGridViewModel>>()

                .AddSingleton<ISessionDataService, SessionDataService>()
                .AddSingleton<ISessionService, SessionService>()
                .AddSingleton<ISalesRecordDataService, SalesRecordDataService>()
                .AddSingleton<ISalesRecordService, SalesRecordService>()
                .AddSingleton<ICategoryDataService, CategoryDataService>()
                .AddSingleton<ICategoryService, CategoryService>() 
                .AddSingleton<IProductDataService, ProductDataService>()
                .AddSingleton<IProductService, ProductService>()
                .AddSingleton<ITransactionsDataService, TransactionsDataService>()
                .AddSingleton<ITransactionsService, TransactionService>()


                .AddTransient<OverviewChartViewModel>()
                .AddTransient<HomeViewModel>()
                .AddTransient<InventoryViewModel>()
                .AddTransient<TransactionViewModel>()
                .AddTransient<SettingsViewModel>()
                .AddTransient<InventoryDataGridViewModel>()

                .AddSingleton<CreateViewModel<ProductDataViewModel>>(s =>
                {
                    return (object parameter) =>
                    {
                        if (parameter is ProductDataViewModelDTO dto)
                            return new ProductDataViewModel(dto);
                        throw new ArgumentException("Invalid parameter type for ProductDataViewModel creation.");
                    };
                })
                .AddSingleton<CreateViewModel<TransactionDataViewModel>>(s =>
                {
                    return (object parameter) =>
                    {
                        if (parameter is TransactionDataViewModelDTO dto)
                        {
                            var inventoryStore = s.GetRequiredService<IInventoryStore>();
                            return new TransactionDataViewModel(dto, inventoryStore);
                        }   

                        throw new ArgumentException("Invalid parameter type for TransactionDataViewModel creation.");
                    };
                })
                .AddSingleton<CreateViewModel<InventoryDataGridViewModel>>(s =>
                {
                    return _ => s.GetRequiredService<InventoryDataGridViewModel>();
                })
                .AddSingleton<CreateViewModel<HomeViewModel>>(s =>
                {
                    return _ => s.GetRequiredService<HomeViewModel>();
                })
                .AddSingleton<CreateViewModel<InventoryViewModel>>(s =>
                {
                    return _ => s.GetRequiredService<InventoryViewModel>();
                })
                .AddSingleton<CreateViewModel<TransactionViewModel>>(s =>
                {
                    return _ => s.GetRequiredService<TransactionViewModel>();
                })
                .AddSingleton<CreateViewModel<SettingsViewModel>>(s =>
                {
                    return _ => s.GetRequiredService<SettingsViewModel>();
                })

                .AddSingleton<ShellViewModel>()
                .AddSingleton<MainWindow>(s => new MainWindow(s.GetRequiredService<ShellViewModel>(), s.GetRequiredService<INavigationServiceEx>()));

            return services.BuildServiceProvider();
        }
    }
}

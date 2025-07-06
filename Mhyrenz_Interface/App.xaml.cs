using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Domain.Services.TransactionService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Domain.State.Mediator;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.Test;
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
        public static IServiceProvider Services { get; set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            Services = CreateServiceProvider();
            IServiceProvider serviceProvider = Services;

            InventoryDbContextFactory contextFactory = serviceProvider.GetRequiredService<InventoryDbContextFactory>();
            using (InventoryDbContext context = contextFactory.CreateDbContext())
            {
                context.Database.Migrate();
            }

            WindowStart(serviceProvider);

            var products = await serviceProvider.GetRequiredService<IProductService>().GetAll();
            //var transactions = await serviceProvider.GetRequiredService<ITransactionsService>().GetLatests();
            //serviceProvider.GetRequiredService<ITransactionStore>().LoadTransactions(transactions);
            serviceProvider.GetRequiredService<IInventoryStore>().LoadProducts(products);
            _ = serviceProvider.GetRequiredService<IBarcodeImageCache>();

            var sessionStore = serviceProvider.GetRequiredService<ISessionStore>();
            var sessionService = serviceProvider.GetRequiredService<ISessionService>();

            var session = await sessionService.GetSession();

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

        private static void WindowStart(IServiceProvider serviceProvider)
        {
            MainWindow mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            //Window testWindow = serviceProvider.GetRequiredService<TestWindow>();
            //testWindow.Show();
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

                .AddSingleton<ICachePath, CachePath>()
                .AddSingleton<IBarcodeImageCache, BarcodeImageCache>(s =>
                {
                    var result = s.GetRequiredService<IInventoryStore>().Products.Select(p =>
                        new Product { Id = p.Item.Id, Barcode = p.Barcode });
                    return BarcodeImageCache.LoadBarcodeImageCache(result, s.GetRequiredService<ICachePath>());
                })

                .AddSingleton<IDialogCoordinator, DialogCoordinator>() // MahApps DIALOG

                .AddSingleton<INavigationServiceEx, NavigationServiceEx>()
                .AddSingleton<IViewModelFactory<NavigationViewModel>, NavigationViewModelFactory>()

                .AddSingleton<IViewModelFactory<ProductDataViewModel>, ViewModelFactory<ProductDataViewModel>>()
                .AddSingleton<IViewModelFactory<TransactionDataViewModel>, ViewModelFactory<TransactionDataViewModel>>()

                .AddSingleton<IViewModelFactory<InventoryDataGridViewModel>, ViewModelFactory<InventoryDataGridViewModel>>()

                .AddSingleton<IViewModelFactory<AddProductViewModel>, ViewModelFactory<AddProductViewModel>>()

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
                .AddTransient<AddProductViewModel>()

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
                .AddSingleton<CreateViewModel<AddProductViewModel>>(s =>
                {
                    return _ => s.GetRequiredService<AddProductViewModel>();
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

                //.AddSingleton<TestWindowViewModel>()
                //.AddSingleton<TestWindow>(s => new TestWindow(s.GetRequiredService<TestWindowViewModel>()));

            return services.BuildServiceProvider();
        }
    }
}

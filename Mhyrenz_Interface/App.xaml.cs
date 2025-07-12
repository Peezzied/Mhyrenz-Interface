using HandyControl.Controls;
using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.ReportsService;
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
using Mhyrenz_Interface.Views;
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
        public delegate TWindow CreateWindow<TWindow>(BaseViewModel viewModel = null) where TWindow : class;
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; set; }
        public static AppPresenter Presenter { get; set; }


        protected override async void OnStartup(StartupEventArgs e)
        {
            Services = CreateServiceProvider();
            IServiceProvider serviceProvider = Services;

            SplashWindow.Init(() => {
                Splash splash = new Splash();
                return splash;
            });

            InventoryDbContextFactory contextFactory = serviceProvider.GetRequiredService<InventoryDbContextFactory>();
            using (InventoryDbContext context = contextFactory.CreateDbContext())
            {
                context.Database.Migrate();
            }



            //await WindowStart(serviceProvider);

            var products = await serviceProvider.GetRequiredService<IProductService>().GetAll();
            //var transactions = await serviceProvider.GetRequiredService<ITransactionsService>().GetLatests();
            //serviceProvider.GetRequiredService<ITransactionStore>().LoadTransactions(transactions);
            serviceProvider.GetRequiredService<IInventoryStore>().LoadProducts(products);
            _ = serviceProvider.GetRequiredService<IBarcodeImageCache>();

            Presenter = new AppPresenter(serviceProvider, Dispatcher);

            await Presenter.ShowStartUpAsync();
            //var sessionStore = serviceProvider.GetRequiredService<ISessionStore>();
            //var sessionService = serviceProvider.GetRequiredService<ISessionService>();

            //var session = await sessionService.GetSession();

            //if (session != null)
            //{
            //    sessionStore.CurrentSession = session;
            //}
            //else
            //{
            //    sessionStore.CurrentSession = sessionService.GenerateSession(new Session { Period = DateTime.Now }).GetAwaiter().GetResult();
            //}

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

                .AddSingleton<ICachePath, CachePath>()
                .AddSingleton<IReportService, ReportService>()
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

                .AddSingleton<IViewModelFactory<SessionBoxContext>, ViewModelFactory<SessionBoxContext>>()

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
                .AddTransient<SessionBoxContext>()

                .AddSingleton<CreateViewModel<ProductDataViewModel>>(s =>
                {
                    return (object parameter) =>
                    {
                        if (parameter is ProductDataViewModelDTO dto)
                            return ActivatorUtilities.CreateInstance<ProductDataViewModel>(s, dto);
                        throw new ArgumentException("Invalid parameter type for ProductDataViewModel creation.");
                    };
                })
                .AddSingleton<CreateViewModel<TransactionDataViewModel>>(s =>
                {
                    return (object parameter) =>
                    {
                        if (parameter is TransactionDataViewModelDTO dto)
                            return ActivatorUtilities.CreateInstance<TransactionDataViewModel>(s, dto);

                        throw new ArgumentException("Invalid parameter type for TransactionDataViewModel creation.");
                    };
                })
                .AddSingleton<CreateViewModel<SessionBoxContext>>(s =>
                {
                    return _ => s.GetRequiredService<SessionBoxContext>();
                })
                .AddSingleton<CreateViewModel<AddProductViewModel>>(s =>
                {
                    return _ => ActivatorUtilities.CreateInstance<AddProductViewModel>(s);
                })
                .AddSingleton<CreateViewModel<InventoryDataGridViewModel>>(s =>
                {
                    return _ => ActivatorUtilities.CreateInstance<InventoryDataGridViewModel>(s);
                })
                .AddSingleton<CreateViewModel<HomeViewModel>>(s =>
                {
                    return _ => ActivatorUtilities.CreateInstance<HomeViewModel>(s);
                })
                .AddSingleton<CreateViewModel<InventoryViewModel>>(s =>
                {
                    return _ => ActivatorUtilities.CreateInstance<InventoryViewModel>(s);
                })
                .AddSingleton<CreateViewModel<TransactionViewModel>>(s =>
                {
                    return _ => ActivatorUtilities.CreateInstance<TransactionViewModel>(s);
                })
                .AddSingleton<CreateViewModel<SettingsViewModel>>(s =>
                {
                    return _ => ActivatorUtilities.CreateInstance<SettingsViewModel>(s);
                })

                .AddSingleton<StartupViewModel>()
                .AddSingleton<ShellViewModel>()
                .AddSingleton<TestWindowViewModel>()

                .AddSingleton<CreateWindow<Startup>>(s =>
                {
                    return (viewModel) =>
                    {
                        return ActivatorUtilities.CreateInstance<Startup>(s, viewModel);
                    };
                })
                .AddSingleton<CreateWindow<MainWindow>>(s =>
                {
                    return (viewModel) =>
                    {
                        return ActivatorUtilities.CreateInstance<MainWindow>(s, viewModel);
                    };
                });

                //.AddTransient<Startup>(s => ActivatorUtilities.CreateInstance<Startup>(s))
                //.AddTransient<MainWindow>(s => ActivatorUtilities.CreateInstance<MainWindow>(s))
                //.AddSingleton<TestWindow>(s => ActivatorUtilities.CreateInstance<TestWindow>(s));


            return services.BuildServiceProvider();
        }
    }
}

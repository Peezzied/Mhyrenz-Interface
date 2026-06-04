using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Converters;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.AppSettingsManager;
using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.ReportsService;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.Test;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Mhyrenz_Interface.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mhyrenz_Interface
{
    public delegate TWindow CreateWindow<TWindow>(BaseViewModel viewModel = null) where TWindow : class;
    public delegate TObject CreateObjectAsync<TObject>(IServiceProvider serviceProvider, Task<TObject> task) where TObject : class;
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _appHost;
        private readonly string _configFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        public static IServiceProvider ServiceProvider { get; set; }
        public static AppPresenter Presenter { get; set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            _appHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile(_configFilePath, optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    CreateServiceCollection(services, context);
                })
                .Build();

            await _appHost.StartAsync();
            ServiceProvider = _appHost.Services;

            using (var context = ServiceProvider.GetRequiredService<InventoryDbContextFactory>().CreateDbContext())
            {
                context.Database.Migrate();
            } // FIXME: TEMPORARY, CHANGE LATER. DEFER TO DATABASE INTIALIZER

            await ServiceProvider.GetRequiredService<AppSettingsManager>()
                .GenerateAppSettings(); // FIXME: TEMPORARY

            ServiceProvider.GetRequiredService<InventorySettingsProvider>().Load();


            Resources.Add("BarcodeToImageConverter",
                ServiceProvider.GetRequiredService<BarcodeToImageConverter>());

            Presenter = new AppPresenter(ServiceProvider);

            await Presenter.AppInit();

            await Presenter.ShowStartUpAsync();
            Presenter.SplashComplete();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void CreateServiceCollection(IServiceCollection services, HostBuilderContext context)
        {

            void inventoryConfig(DbContextOptionsBuilder options)
            {
                options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")); // FIXME: TEMPORARY, CHANGE LATER
            }

            services.AddOptions<AppSettingsManager.AppSettings>()
                .BindConfiguration("AppSettings");

            services.AddOptions<List<InventorySettings>>()
                .BindConfiguration("InventorySettings");

            services
                .AddSingleton(new AppSettingsManager.FilePath(_configFilePath))
                .AddSingleton<AppSettingsManager>()

                .AddDbContext<InventoryDbContext>(inventoryConfig)
                .AddSingleton(new InventoryDbContextFactory(inventoryConfig))

                .AddSingleton<InventorySettingsProvider>()

                .AddSingleton<InventoryDataGridSettingsProvider>()

                .AddSingleton<BarcodeToImageConverter>()

                .AddSingleton<IUndoRedoManager, UndoRedoManager>()
                .AddSingleton<ISerialBarcodeService, SerialBarcodeService>()
                .AddSingleton<ISessionStore, SessionStore>()
                .AddSingleton<ISessionStore, SessionStore>()
                .AddSingleton<IInventoryStore, InventoryStore>()
                .AddSingleton<ICategoryStore, CategoryStore>()
                .AddSingleton<ITransactionStore, TransactionStore>()

                .AddSingleton<ICachePath, CachePath>()
                .AddSingleton<IBarcodeImageCache, BarcodeImageCache>()
                .AddSingleton<IReportService, ReportService>()

                .AddSingleton<IDialogCoordinator, DialogCoordinator>() // MahApps DIALOG

                .AddSingleton<INavigationServiceEx, NavigationServiceEx>()
                .AddSingleton<NavigationViewModelFactory>()

                .AddSingleton<ISessionService, SessionService>()
                .AddSingleton<ICheckoutService, CheckoutService>()
                .AddSingleton<ICategoryService, CategoryService>()
                .AddSingleton<IProductService, ProductService>()
                .AddSingleton<IOrderStore, OrderStore>()

                .AddTransient<IncomingPanelViewModel>()
                .AddTransient<OverviewChartViewModel>()

                .AddViewModelFactory<ProductDataViewModel, Product>()
                .AddViewModelFactory<TransactionDataViewModel, Transaction>()
                .AddViewModelFactory<ColumnSettingViewModel, ColumnSetting>()
                .AddViewModelFactory<OrderViewModel, Order>()
                .AddViewModelFactory<PlaceOrderViewModel>()
                .AddViewModelFactory<InventoryDataGridViewModel>()
                .AddViewModelFactory<InventoryTabItem>()
                .AddViewModelFactory<SaleTabItem>()
                .AddViewModelFactory<SessionBoxContext>(resolveFromContainer: true)
                .AddViewModelFactory<AddProductViewModel>(resolveFromContainer: true)
                .AddViewModelFactory<CompletedSaleViewModel>(resolveFromContainer: true)

                .AddViewModelFactory<HomeViewModel>(resolveFromContainer: true)
                .AddViewModelFactory<InventoryViewModel>(resolveFromContainer: true)
                .AddViewModelFactory<CheckoutViewModel>(resolveFromContainer: true)
                .AddViewModelFactory<SettingsViewModel>(resolveFromContainer: true)

                .AddCommandFactory<DeleteCommand>()
                .AddCommandFactory<AddCommand>()
                .AddCommandFactory<CreateSessionCommand>()
                .AddCommandFactory<LoadCategoriesCommand>()
                .AddCommandFactory<CheckoutCommand>()
                .AddCommandFactory<TransactionVMCommandDiscount, TransactionVMCommandDiscount.DTO>()
                .AddCommandFactory<TransactionVMCommandPurchase, TransactionVMCommandPurchase.DTO>()
                .AddCommandFactory<ProductVMCommandCommonProp, ProductVMCommandCommonProp.DTO>()
                .AddCommandFactory<ProductVMCommandPurchase, ProductVMCommandPurchase.DTO>()
                .AddCommandFactory<PlaceOrderVMCommandQty, PlaceOrderVMCommandQty.DTO>()

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
        }
    }
}

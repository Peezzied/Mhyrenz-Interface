using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface.Bootstrap;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Core.Utilities;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.ReportsService;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Domain.Services.Settings;
using Mhyrenz_Interface.Features.Checkout.Commands;
using Mhyrenz_Interface.Features.Checkout.ViewModels;
using Mhyrenz_Interface.Features.Home.ViewModels;
using Mhyrenz_Interface.Features.Inventory.Commands;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Features.Orders.Commands;
using Mhyrenz_Interface.Features.Orders.ViewModels;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.Shared.Converters;
using Mhyrenz_Interface.Startup;
using Mhyrenz_Interface.Startup.ViewModels;
using Mhyrenz_Interface.Store;
using Mhyrenz_Interface.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Mhyrenz_Interface
{
    public delegate TWindow CreateWindow<TWindow>(BaseViewModel viewModel = null) where TWindow : class;
    public delegate TObject CreateObjectAsync<TObject>(IServiceProvider serviceProvider, Task<TObject> task) where TObject : class;
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        private IHost _appHost;
        private readonly string _appsettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        public static IServiceProvider ServiceProvider { get; set; }
        public static AppPresenter Presenter { get; set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            _appHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile(_appsettingsPath, optional: false, reloadOnChange: true);
#if DEBUG
                    config.AddUserSecrets<App>();
#endif
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

            ServiceProvider.GetRequiredService<ConfigManager<AppSettings>>()
                .GenerateConfig(nameof(AppSettings)); // FIXME: TEMPORARY

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

            services.AddOptions<AppSettings>()
                .BindConfiguration(nameof(AppSettings));

            services.AddOptions<InventoryDataGridSettings>()
                .BindConfiguration(nameof(InventoryDataGridSettings));

            services.AddOptions<TelegramSettings>()
                .BindConfiguration("Telegram");

            services
                .AddSingleton(s => ActivatorUtilities.CreateInstance<ConfigManager<AppSettings>>(s, _appsettingsPath))

                .AddDbContext<InventoryDbContext>(inventoryConfig)
                .AddSingleton(new InventoryDbContextFactory(inventoryConfig))

                 .AddSingleton<ITelegramBotService>(s =>
                 {
                     var telegram = s.GetRequiredService<IOptions<TelegramSettings>>().Value;

                     return new TelegramBotService(telegram.BotToken, telegram.ChatId);
                 })

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
                .AddSingleton<IOrderService, OrderService>()
                .AddSingleton<IOrderStore, OrderStore>()

                .AddTransient<IncomingPanelViewModel>()
                .AddTransient<OverviewChartViewModel>()

                .AddViewModelFactory<ProductDataViewModel, Product>()
                .AddViewModelFactory<TransactionDataViewModel, Transaction>()
                .AddViewModelFactory<ColumnSettingViewModel, InventoryDataGridColumnSetting>()
                .AddViewModelFactory<OrderDataViewModel, Order>()
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

                .AddSingleton<CreateWindow<StartupWindow>>(s =>
                {
                    return (viewModel) =>
                    {
                        return ActivatorUtilities.CreateInstance<StartupWindow>(s, viewModel);
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

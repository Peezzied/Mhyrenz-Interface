using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using MahApps.Metro.Controls.Dialogs;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.AppSettingsManager;
using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Domain.Services.ReportsService;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.SerialBarcodeService;
using Mhyrenz_Interface.Domain.Services.SessionService;
using Mhyrenz_Interface.Domain.Services.TransactionService;
using Mhyrenz_Interface.Domain.State;
using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.Test;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
using Mhyrenz_Interface.Views;
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

            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

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

            await ServiceProvider.GetRequiredService<AppSettingsManager>()
                .GenerateAppSettings(); // TEMPORARY

            ServiceProvider.GetRequiredService<InventorySettingsProvider>().Load();

            Presenter = new AppPresenter(ServiceProvider);

            await Presenter.AppInit();

            await Presenter.ShowStartUpAsync();
            Presenter.SplashComplete();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceProvider.GetRequiredService<InventoryDbService>().Dispose();
            base.OnExit(e);
        }

        private void CreateServiceCollection(IServiceCollection services, HostBuilderContext context)
        {

            var inventoryConfig = context.Configuration.GetConnectionString("DefaultConnection");

            services.AddOptions<AppSettingsManager.Settings>()
                .BindConfiguration("AppSettings");

            services.AddOptions<List<InventorySettings>>()
                .BindConfiguration("Inventory");

            services
                .AddSingleton(new AppSettingsManager.FilePath(_configFilePath))
                .AddSingleton<AppSettingsManager>()

                .AddSingleton<InventoryDbService>(new InventoryDbService(inventoryConfig))

                .AddSingleton<InventorySettingsProvider>()

                .AddSingleton<IUndoRedoManager, UndoRedoManager>()
                .AddSingleton<ISerialBarcodeService, SerialBarcodeService>()
                .AddSingleton<ISessionStore, SessionStore>()
                .AddSingleton<ISessionStore, SessionStore>()
                .AddSingleton<IInventoryStore, InventoryStore>()
                .AddSingleton<ITransactionStore, TransactionStore>()
                .AddSingleton<ICategoryStore, CategoryStore>()

                .AddSingleton<ICachePath, CachePath>()
                .AddSingleton<IBarcodeImageCache, BarcodeImageCache>()
                .AddSingleton<IReportService, ReportService>()

                .AddSingleton<IDialogCoordinator, DialogCoordinator>() // MahApps DIALOG

                .AddSingleton<INavigationServiceEx, NavigationServiceEx>()
                .AddSingleton<NavigationViewModelFactory>()

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

                .AddTransient<IncomingPanelViewModel>()

                .AddTransient<OverviewChartViewModel>()
                .AddTransient<HomeViewModel>()
                .AddTransient<InventoryViewModel>()
                .AddTransient<TransactionViewModel>()
                .AddTransient<SettingsViewModel>()
                .AddTransient<InventoryDataGridViewModel>()
                .AddTransient<AddProductViewModel>()
                .AddTransient<SessionBoxContext>()

                //.AddSingleton<CreateViewModel<IncomingPanelViewModel>>(s =>
                //{
                //    return _ => ActivatorUtilities.CreateInstance<IncomingPanelViewModel>(s);
                //})
                .AddSingleton<CreateViewModel<ProductDataViewModel>>(s =>
                {
                    return (object parameter) =>
                    {
                        if (parameter is Product product)
                            return ActivatorUtilities.CreateInstance<ProductDataViewModel>(s, product);
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
                    return (object parameter) =>
                    {
                        if (parameter is BaseViewModel vm)
                            return ActivatorUtilities.CreateInstance<InventoryDataGridViewModel>(s, vm);
                        throw new ArgumentException("Invalid parameter type for InventoryDataGridViewModel creation.");
                    };
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
        }
    }
}

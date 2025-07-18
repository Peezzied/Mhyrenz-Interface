using HandyControl.Controls;
using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;
using Mhyrenz_Interface.State;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Window = System.Windows.Window;

namespace Mhyrenz_Interface
{
    public class AppPresenter
    {
        private Window _currentWindow;
        private SplashWindow _splash;
        private IServiceProvider _serviceProvider;
        private readonly IServiceCollection _serviceCollection;
        private readonly Dispatcher _dispatcher;
        private readonly StartupManager _startupManager;

        public AppPresenter(IServiceCollection services, IServiceProvider serviceProvider, Dispatcher dispatcher)
        {
            _serviceProvider = serviceProvider;
            _serviceCollection = services;
            _dispatcher = dispatcher;

            StartupManager.Register(new StartupAction("Inventory Store", "Fetching data from database", async (sp) => await InventoryStore.LoadInventoryStore(sp)));
            StartupManager.Register(new StartupAction("Transactions Store", "Loading transactions from cache", async (sp) => await TransactionStore.LoadTransactionStore(sp)));
            StartupManager.Register(new StartupAction("Categories Store", "Categorizing inventory from cache", async (sp) => await CategoryStore.LoadCategoryStore(sp)));
            StartupManager.Register(new StartupAction("Barcode Image Caching", "Caching barcodes",
                async (sp) =>
                {
                    var products = sp.GetRequiredService<IInventoryStore>().Products.Select(p => p.Item);
                    var barcodeCache = sp.GetRequiredService<IBarcodeImageCache>();
                    await BarcodeImageCache.LoadBarcodeImageCache(products, barcodeCache);
                }));
        }

        public async Task ShowStartUpAsync()
        {
            var vm = await StartupViewModel.LoadStartupViewModel(_serviceProvider);
            var startUp = _serviceProvider.GetRequiredService<CreateWindow<Startup>>().Invoke(vm);
            ShowWindow(startUp);
        }

        public async Task ShowMainWindowAsync()
        {
            var vm = await ShellViewModel.LoadMainViewModel(_serviceProvider);
            var mainWindow = _serviceProvider.GetRequiredService<CreateWindow<MainWindow>>().Invoke(vm);
            ShowWindow(mainWindow);
        }
        private void ShowWindow(Window startUp)
        {
            var oldWindow = _currentWindow;
            _currentWindow = startUp;
            _currentWindow.Show();
            oldWindow?.Close();
        }

        internal async Task<IServiceProvider> AppInit()
        {
            SplashWindow.Init(() =>
            {
                Splash splash = new Splash();
                return splash;
            });

            var provider = await StartupManager.Init(_serviceProvider, SplashWindow.Instance);
            return provider;
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using HandyControl.Controls;
using HandyControl.Tools;
using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Startup;
using Mhyrenz_Interface.Startup.ViewModels;
using Mhyrenz_Interface.Store;
using Microsoft.Extensions.DependencyInjection;
using Window = System.Windows.Window;

namespace Mhyrenz_Interface.Bootstrap
{
    public class AppPresenter
    {
        private Window _currentWindow;
        private readonly IServiceProvider _serviceProvider;

        public AppPresenter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            StartupManager.Register(new StartupManager.Action("Inventory Store", "Fetching data from database", async (sp) => await InventoryStore.LoadInventoryStore(sp)));
            StartupManager.Register(new StartupManager.Action("Transaction Store", "Fetching data from database", async (sp) => await TransactionStore.LoadTransactionStore(sp)));
            StartupManager.Register(new StartupManager.Action("Categories Store", "Categorizing inventory from cache", async (sp) => await CategoryStore.LoadCategoryStore(sp)));
            StartupManager.Register(new StartupManager.Action("Utility", "Deleting items",
                async (sp) =>
                {
                    var count = await sp.GetRequiredService<IProductService>().RemovePhysical();
                    return (count == 0) ? "No pending items delete." : $"Deleted {count} items successfully.";
                }));
            StartupManager.Register(new StartupManager.Action("Barcode Image Caching", "Caching barcodes",
                async (sp) =>
                {
                    var products = sp.GetRequiredService<IInventoryStore>().Store.Select(p => p.Item);
                    var barcodeCache = sp.GetRequiredService<IBarcodeImageCache>();
                    await BarcodeImageCache.LoadBarcodeImageCache(products, barcodeCache);
                }));
        }

        public async Task ShowStartUpAsync()
        {
            var vm = await StartupViewModel.LoadStartupViewModel(_serviceProvider);
            var startUp = _serviceProvider.GetRequiredService<CreateWindow<StartupWindow>>().Invoke(vm);
            ShowWindow(startUp);
        }

        public void ShowMainWindowAsync()
        {
            var vm = _serviceProvider.GetRequiredService<ShellViewModel>();
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

        internal void SplashComplete()
        {
            SplashWindow.Instance.RunOnUIThread(() => SplashWindow.Instance.Close());
            SplashWindow.Instance.LoadComplete();
        }
    }
}

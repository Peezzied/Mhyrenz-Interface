using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Mhyrenz_Interface.Navigation;
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

            base.OnStartup(e);
        }

        private IServiceProvider CreateServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            Action<DbContextOptionsBuilder> configureDbContext = options =>
            {
                options.UseSqlite("Data Source=dev_inventory.db");
            };

            services
                .AddDbContext<InventoryDbContext>(configureDbContext)
                .AddSingleton<InventoryDbContextFactory>(new InventoryDbContextFactory(configureDbContext))

                .AddSingleton<INavigationServiceEx, NavigationServiceEx>()
                .AddSingleton<IViewModelFactory, ViewModelFactory>()

                .AddSingleton<IProductDataService, ProductDataService>()
                .AddSingleton<IProductService, ProductService>()

                .AddTransient<HomeViewModel>()
                .AddTransient<InventoryViewModel>()
                .AddTransient<TransactionsViewModel>()
                .AddTransient<SettingsViewModel>()

                .AddSingleton<CreateViewModel<HomeViewModel>>(s =>
                {
                    return () => s.GetRequiredService<HomeViewModel>();
                })
                .AddSingleton<CreateViewModel<InventoryViewModel>>(s =>
                {
                    return () => s.GetRequiredService<InventoryViewModel>();
                })
                .AddSingleton<CreateViewModel<TransactionsViewModel>>(s =>
                {
                    return () => s.GetRequiredService<TransactionsViewModel>();
                })
                .AddSingleton<CreateViewModel<SettingsViewModel>>(s =>
                {
                    return () => s.GetRequiredService<SettingsViewModel>();
                })

                .AddSingleton<ShellViewModel>()
                .AddSingleton<MainWindow>(s => new MainWindow(s.GetRequiredService<ShellViewModel>(), s.GetRequiredService<INavigationServiceEx>()));

            return services.BuildServiceProvider();
        }
    }
}

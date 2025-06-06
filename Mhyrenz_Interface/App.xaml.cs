using Mhyrenz_Interface.Navigation;
using Mhyrenz_Interface.ViewModels;
using Mhyrenz_Interface.ViewModels.Factory;
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
            
            MainWindow mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private IServiceProvider CreateServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            services
                .AddSingleton<INavigationServiceEx, NavigationServiceEx>()
                .AddSingleton<IViewModelFactory, ViewModelFactory>()


                .AddSingleton<HomeViewModel>()
                .AddSingleton<InventoryViewModel>()
                .AddSingleton<TransactionsViewModel>()
                .AddSingleton<SettingsViewModel>()

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

                .AddScoped<ShellViewModel>()
                .AddScoped<MainWindow>(s => new MainWindow(s.GetRequiredService<ShellViewModel>()));

            return services.BuildServiceProvider();
        }
    }
}

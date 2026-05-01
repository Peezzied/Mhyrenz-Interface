using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Mhyrenz_Interface.Core;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.AppSettingsManager;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.Domain.Services.TransactionService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Mhyrenz_Interface.Test.AppSettingsTestSetup
{
    [SetUpFixture]
    public class AppSettingsTestSetup
    {
        private static IHost _appHost;
        public static IServiceProvider ServiceProvider;
        private static readonly string _configFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        [OneTimeSetUp]
        public static async Task Setup()
        {
            _appHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile(_configFilePath, optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var inventoryConfig = context.Configuration.GetConnectionString("DefaultConnection");

                    services.AddOptions<AppSettingsManager.AppSettings>()
                        .BindConfiguration("AppSettings");

                    services.AddOptions<List<InventorySettings>>()
                        .BindConfiguration("InventorySettings");

                    services
                        .AddSingleton(new AppSettingsManager.FilePath(_configFilePath))
                        .AddSingleton<AppSettingsManager>()
                        .AddSingleton<InventoryDbService>(new InventoryDbService(inventoryConfig))

                        .AddSingleton<InventorySettingsProvider>()

                        .AddSingleton<ICategoryDataService, CategoryDataService>()
                        .AddSingleton<ICategoryService, CategoryService>()
                        .AddSingleton<ITransactionsDataService, TransactionsDataService>()
                        .AddSingleton<ITransactionsService, TransactionService>();
                })
                .Build();
            ServiceProvider = _appHost.Services;
            await ServiceProvider.GetRequiredService<AppSettingsManager>().GenerateAppSettings();
            AppSettingsTestSetup.ServiceProvider.GetRequiredService<InventorySettingsProvider>().Load();
        }
    }

    [TestFixture]
    public class AppSettingsTest
    {
        [Test]
        public async Task GenerateAppSettingsTest()
        {
            await AppSettingsTestSetup.ServiceProvider.GetRequiredService<AppSettingsManager>().GenerateAppSettings();
        }

        [Test]
        public async Task AppSettingsIOptionsMonitorTest()
        {
            Assert.That(AppSettingsTestSetup.ServiceProvider.GetRequiredService<IOptionsMonitor<List<InventorySettings>>>().CurrentValue, Is.Not.Null);
        }

        [Test]
        public async Task UpdateAppSettingsNodeTest()
        {
            AppSettingsTestSetup.ServiceProvider.GetRequiredService<AppSettingsManager>().UpdateAppSettingsNode<AppSettingsManager.AppSettings>(options =>
            {
                options.BarcodePort = "IT WORKED";
            });
        }
    }
}

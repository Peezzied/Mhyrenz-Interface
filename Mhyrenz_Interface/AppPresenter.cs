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
using System.Windows.Threading;

namespace Mhyrenz_Interface
{
    public class AppPresenter
    {
        private Window _currentWindow;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dispatcher _dispatcher;

        public AppPresenter(IServiceProvider serviceProvider, Dispatcher dispatcher)
        {
            _serviceProvider = serviceProvider;
            _dispatcher = dispatcher;
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
            _currentWindow?.Close();
            _currentWindow = startUp;
            _currentWindow.Show();
        }
    }
}

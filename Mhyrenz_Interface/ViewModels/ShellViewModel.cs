using MahApps.Metro.IconPacks;
using Mhyrenz_Interface.Views;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Mhyrenz_Interface.ViewModels
{
    public class ShellViewModel : BaseViewModel
    {
        private static readonly ObservableCollection<MenuItem> AppMenu = new ObservableCollection<MenuItem>();
        private static readonly ObservableCollection<MenuItem> AppOptionsMenu = new ObservableCollection<MenuItem>();

        public ObservableCollection<MenuItem> Menu => AppMenu;

        public ObservableCollection<MenuItem> OptionsMenu => AppOptionsMenu;

        public ShellViewModel()
        {



            // Build the menus
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.HouseSolid },
                Label = "Home",
                NavigationType = typeof(HomeView),
                NavigationDestination = new Uri("Views/HomeView.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FolderSolid },
                Label = "Inventory",
                NavigationType = typeof(Inventory),
                NavigationDestination = new Uri("Views/Inventory.xaml", UriKind.RelativeOrAbsolute)
            });
            this.OptionsMenu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.GearSolid },
                Label = "Settings",
                NavigationType = typeof(Settings),
                NavigationDestination = new Uri("Views/Settings.xaml", UriKind.RelativeOrAbsolute)
            });
        }
    }
}

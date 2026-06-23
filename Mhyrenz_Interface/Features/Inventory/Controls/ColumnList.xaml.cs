using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using Dragablz;
using Mhyrenz_Interface.Domain.Models.Settings;
using Mhyrenz_Interface.Domain.Services.Settings;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Store;

namespace Mhyrenz_Interface.Features.Inventory.Controls
{
    /// <summary>
    /// Interaction logic for ColumnList.xaml
    /// </summary>
    public partial class ColumnList : UserControl
    {
        public ObservableCollection<Column> Columns { get; set; }
        public ColumnList()
        {
            InitializeComponent();

            Columns = new ObservableCollection<Column>
            {
                new Column { Name = "Generic", Color = Brushes.Red },
                new Column { Name = "Branded", Color = Brushes.Green },
                new Column { Name = "Cosmetics", Color = Brushes.Blue },
            };
            DragablzItemsControl.AddHandler(DragablzItem.DragCompleted, new DragablzDragCompletedEventHandler(OnDragCompleted), true);
        }

        private void OnDragCompleted(object sender, DragablzDragCompletedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                var tab = ((InventoryTabItem)DataContext);
                var pharmaColumns = InventoryDataGridSettingsStore.Load().Where(s => s.PharmaColumn);

                var list = DragablzItemsControl.ItemsOrganiser.Sort(DragablzItemsControl.Items.Cast<object>()
                  .Select(item => DragablzItemsControl.ItemContainerGenerator.ContainerFromItem(item) as DragablzItem))
                  .Select(x => new
                  {
                      x.LogicalIndex,
                      SettingVm = (ColumnSettingViewModel)x.Content,
                      ((ColumnSettingViewModel)x.Content).ColumnSetting
                  });

                if (!tab.IsPharma)
                {
                    list = list.Concat(pharmaColumns.Select(p => new
                    {
                        LogicalIndex = p.DisplayIndex,
                        SettingVm = (ColumnSettingViewModel)null,
                        ColumnSetting = p
                    }));
                }

                list = list.OrderBy(x => x.LogicalIndex);

                var index = 0;
                foreach (var item in list)
                {
                    item.ColumnSetting.DisplayIndex = index;

                    if (item.SettingVm != null)
                        item.SettingVm.DisplayIndex = index;

                    index++;
                }

                InventoryDataGridSettingsStore.Save(new InventoryDataGridSettings(list.Select(x => x.ColumnSetting)));
                tab.OnColumnsChanged();

            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
    }

    public class Column : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public Brush Color { get; set; }

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; OnPropertyChanged(); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

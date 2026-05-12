using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dragablz;
using Mhyrenz_Interface.ViewModels;

namespace Mhyrenz_Interface.Controls
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
                var list = DragablzItemsControl.ItemsOrganiser.Sort(DragablzItemsControl.Items.Cast<object>()
                  .Select(item => DragablzItemsControl.ItemContainerGenerator.ContainerFromItem(item) as DragablzItem))
                  .OrderBy(di => di.LogicalIndex)
                  .ToHashSet();


                //var sortedIndices = list
                //    .Select(i => ((ColumnSettingViewModel)i.Content).DisplayIndex)
                //    .OrderBy(i => i)
                //    .ToList();

                for (int i = 0; i < list.Count(); i++)
                {
                    var item = (ColumnSettingViewModel)list.ElementAt(i).Content;
                    item.DisplayIndex = i + 1;
                }
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mhyrenz_Interface.WpfTest
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

            DataContext = this;
        }
    }

    public class Column: INotifyPropertyChanged
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

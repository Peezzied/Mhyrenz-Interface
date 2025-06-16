using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Points;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Mhyrenz_Interface.Controls
{
    /// <summary>
    /// Interaction logic for OverviewChart.xaml
    /// </summary>
    public partial class OverviewChart : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();

        public OverviewChart()
        {
            InitializeComponent();
        }

        private void PieChart_DataHover(object sender, ChartPoint chartPoint)
        {
            var slice = (PieSlice)sender;

            slice.Opacity = 0;
        }

        private void PieChart_MouseLeave(object sender, MouseEventArgs e)
        {
            foreach (var series in ((PieChart)sender).Series.Cast<Series>())
            {
                series.Opacity = 1.0; // Reset to full opacity
            }

        }
    }
}

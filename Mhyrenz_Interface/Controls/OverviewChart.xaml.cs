﻿using LiveCharts;
using LiveCharts.Definitions.Charts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Points;
using Mhyrenz_Interface.Controls.Tooltips;
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
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using HandyControl.Tools.Extension;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Mhyrenz_Interface.Core.MVVM;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Features.Inventory.ViewModels;
using Mhyrenz_Interface.Store;
using SkiaSharp;

namespace Mhyrenz_Interface.Features.Home.ViewModels
{
    public class CategoryChartViewModel : INotifyPropertyChanged
    {
        public Category Category { get; set; }
        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableValue _sales;
        public ObservableValue Sales
        {
            get => _sales;
            set
            {
                _sales = value;
                OnPropertyChanged(nameof(Sales));
            }
        }



        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class OverviewChartViewModel : BaseViewModel
    {
        private readonly ICategoryStore _categoryStore;
        private readonly ITransactionStore _transactionStore;

        public ObservableCollection<PieSeries<ObservableValue>> SalesByCategory { get; private set; }
            = new ObservableCollection<PieSeries<ObservableValue>>();

        public bool HasSales => SalesByCategory.Where(c => c.Values.First().Value > 0).Any();

        public List<CategoryChartViewModel> CategoryChartData
            = new List<CategoryChartViewModel>();

        public OverviewChartViewModel(ICategoryStore categoryStore, ITransactionStore transactionStore)
        {
            _categoryStore = categoryStore;
            _transactionStore = transactionStore;

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadChart();
            }));
        }

        public override void Dispose()
        {
            foreach (var item in SalesByCategory)
            {
                item.PointCreated -= Item_PointCreated;
            }
        }

        private void Item_PointCreated(ChartPoint<ObservableValue, LiveChartsCore.SkiaSharpView.Drawing.Geometries.DoughnutGeometry, LiveChartsCore.SkiaSharpView.Drawing.Geometries.LabelGeometry> obj)
        {
            _categoryStore.Colors[obj.Context.Series.Tag.CastTo<int>()] = new BrushConverter()
                .ConvertFromString((obj.Context.Series as PieSeries<ObservableValue>).Fill
                    .CastTo<SolidColorPaint>().Color.ToString())
                .CastTo<SolidColorBrush>();
        }

        private void LoadChart()
        {
            CategoryChartData.Clear();

            var categoryBySales = _transactionStore.Store
                .GroupBy(t => t.Product.CategoryId)
                .ToDictionary(k => k.Key, v => (double)v.Sum(t => t.TotalPrice));

            var chartData = _categoryStore.Categories.Select(category => new CategoryChartViewModel
            {
                Category = category.Value,
                Name = category.Value.Name,
                Sales = new ObservableValue(categoryBySales.TryGetValue(category.Value.Id, out var sales)
                    ? sales
                    : 0)
            });

            CategoryChartData.AddRange(chartData);

            if (SalesByCategory.Any())
                return;

            var pieSeries = CategoryChartData.Select(c => new PieSeries<ObservableValue>
            {
                Values = new ObservableCollection<ObservableValue> { c.Sales },
                Name = c.Name,
                Tag = c.Category.Id,
                IsVisibleAtLegend = c.Sales.Value > 0,
                InnerRadius = 50,
                ToolTipLabelFormatter = point => $"{point.Label.Text} {point.Model.Value:C}",
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsSize = c.Sales.Value > 0 ? 14 : 0,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point =>
                {
                    var sv = point.StackedValue;
                    return $"{sv.Share:P2}{Environment.NewLine}{point.Model.Value:C}";
                }
            });

            SalesByCategory.AddRange(pieSeries);

            foreach (var item in SalesByCategory)
            {
                item.PointCreated += Item_PointCreated;
            }
        }
    }
}

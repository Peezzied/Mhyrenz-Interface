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
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.State;
using SkiaSharp;

namespace Mhyrenz_Interface.ViewModels
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
                OnPropertyChanged("Sales");
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
        private readonly IInventoryStore _inventoryStore;

        public string Bindtest { get; set; } = "Hello, World! from OverviewChartViewModel!";
        public ObservableCollection<PieSeries<ObservableValue>> SalesByCategory { get; private set; } = new ObservableCollection<PieSeries<ObservableValue>>();
        public Dictionary<Category, ICollectionView> Categories => _categoryStore.Categories;

        public ObservableCollection<CategoryChartViewModel> CategoryChartData = new ObservableCollection<CategoryChartViewModel>();

        public OverviewChartViewModel(ICategoryStore categoryStore, IInventoryStore inventoryStore)
        {
            _categoryStore = categoryStore;
            _inventoryStore = inventoryStore;

            _inventoryStore.PurchaseEvent += InventoryStore_PurchaseEvent;
            _inventoryStore.Loaded += InventoryStore_Loaded;

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadChart(Categories);
            }));
        }

        private void Item_PointCreated(ChartPoint<ObservableValue, LiveChartsCore.SkiaSharpView.Drawing.Geometries.DoughnutGeometry, LiveChartsCore.SkiaSharpView.Drawing.Geometries.LabelGeometry> obj)
        {
            _categoryStore.Colors[obj.Context.Series.Tag.CastTo<int>()] = new BrushConverter().ConvertFromString((obj.Context.Series as PieSeries<ObservableValue>).Fill.CastTo<SolidColorPaint>().Color.ToString()).CastTo<SolidColorBrush>();
        }

        private void InventoryStore_Loaded()
        {
            RefreshChart();
        }

        public override void Dispose()
        {
            _inventoryStore.PurchaseEvent -= InventoryStore_PurchaseEvent;
            _inventoryStore.Loaded -= InventoryStore_Loaded;
        }
        private void InventoryStore_PurchaseEvent(object sender, InventoryStoreEventArgs e)
        {
            RefreshChart();
        }

        private void RefreshChart()
        {
            foreach (var item in CategoryChartData)
            {
                item.Sales.Value = (double)Categories[item.Category].Cast<ProductDataViewModel>()
                    .Where(p => p.Purchase > 0)
                    .Sum(x => x.NetRetailPrice);
            }
        }

        private void LoadChart(Dictionary<Category, ICollectionView> categories)
        {
            CategoryChartData.Clear();
            //SalesByCategory.Clear();

            var chartData = categories.Select(c => new CategoryChartViewModel()
            {
                Category = c.Key,
                Name = c.Key.Name,
                Sales = new ObservableValue((double)c.Value.Cast<ProductDataViewModel>()
                    .Where(p => p.Purchase > 0)
                    .Sum(x => x.NetRetailPrice))
            });

            CategoryChartData.AddRange(chartData);

            if (SalesByCategory.Any())
                return;

            var pieSeries = CategoryChartData.Select(c =>
            {

                return new PieSeries<ObservableValue>()
                {
                    Values = new ObservableCollection<ObservableValue> { c.Sales },
                    Name = c.Name,
                    Tag = c.Category.Id,
                    IsVisibleAtLegend = !(c.Sales.Value <= 0),
                    InnerRadius = 50,
                    ToolTipLabelFormatter = point => $"{point.Label.Text} {point.Model.Value:C}",
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = !(c.Sales.Value <= 0) ? 14 : 0,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter =
                        point =>
                        {
                            var pv = point.Coordinate.PrimaryValue;
                            var sv = point.StackedValue;

                            var a = $"{sv.Share:P2}{Environment.NewLine}{point.Model.Value:C}";
                            return a;
                        }
                };
            });

            SalesByCategory.AddRange(pieSeries);

            foreach (var item in SalesByCategory)
            {
                item.PointCreated += Item_PointCreated;
            }
        }
    }
}

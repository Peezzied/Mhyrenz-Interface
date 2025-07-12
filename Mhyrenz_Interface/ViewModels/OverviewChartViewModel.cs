using HandyControl.Tools.Extension;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Layouts;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Layouts;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.VisualElements;
using MahApps.Metro.Controls;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.State;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Mhyrenz_Interface.ViewModels
{
    public class CategoryChartViewModel: INotifyPropertyChanged
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
    public class OverviewChartViewModel: BaseViewModel
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

            LoadChart(Categories);

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
                    DataLabelsSize = 14,
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
            SetColors();
        }

        private void SetColors()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (SalesByCategory.Count == _categoryStore.Colors.Count)
                    return;

                foreach (var item in SalesByCategory)
                {
                    if (item.Fill is null)
                        return;

                    _categoryStore.Colors[item.Tag.CastTo<int>()] = 
                        new BrushConverter().ConvertFromString(item.Fill.CastTo<SolidColorPaint>().Color.ToString()).CastTo<SolidColorBrush>();
                }
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
    }
}

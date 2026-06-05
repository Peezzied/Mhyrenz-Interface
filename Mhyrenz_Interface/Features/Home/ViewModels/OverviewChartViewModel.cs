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
        public ObservableCollection<PieSeries<ObservableValue>> SalesByCategory { get; private set; }
            = new ObservableCollection<PieSeries<ObservableValue>>();

        public ObservableCollection<CategoryChartViewModel> CategoryChartData
            = new ObservableCollection<CategoryChartViewModel>();

        public OverviewChartViewModel(ICategoryStore categoryStore, IInventoryStore inventoryStore)
        {
            _categoryStore = categoryStore;
            _inventoryStore = inventoryStore;

            _inventoryStore.PurchaseEvent += InventoryStore_PurchaseEvent;
            _inventoryStore.Loaded += InventoryStore_Loaded;

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadChart();
            }));
        }

        // No longer needs to create ICollectionView per category —
        // just filter the dictionary values directly
        private IEnumerable<ProductDataViewModel> GetProductsByCategory(Category category)
        {
            return _inventoryStore.Store
                .Where(p => p.CategoryId == category.Id);
        }

        private decimal GetSalesForCategory(Category category)
        {
            return GetProductsByCategory(category)
                .Where(p => p.Purchase > 0)
                .Sum(x => x.NetRetailPrice);
        }

        private void Item_PointCreated(ChartPoint<ObservableValue, LiveChartsCore.SkiaSharpView.Drawing.Geometries.DoughnutGeometry, LiveChartsCore.SkiaSharpView.Drawing.Geometries.LabelGeometry> obj)
        {
            _categoryStore.Colors[obj.Context.Series.Tag.CastTo<int>()] = new BrushConverter()
                .ConvertFromString((obj.Context.Series as PieSeries<ObservableValue>).Fill
                    .CastTo<SolidColorPaint>().Color.ToString())
                .CastTo<SolidColorBrush>();
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
                item.Sales.Value = (double)GetSalesForCategory(item.Category);
            }
        }

        private void LoadChart()
        {
            CategoryChartData.Clear();

            var chartData = _categoryStore.Categories.Select(category => new CategoryChartViewModel
            {
                Category = category.Value,
                Name = category.Value.Name,
                Sales = new ObservableValue((double)GetSalesForCategory(category.Value))
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

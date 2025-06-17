using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.WPF;
using Mhyrenz_Interface.Commands;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.State;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.ViewModels
{
    public class CategoryChartViewModel
    {
        public string Name { get; set; }
        public double ProductCount { get; set; }
    }

    public class OverviewChartViewModel: BaseViewModel
    {
        private readonly ICategoryStore _categoryStore;
        public string Bindtest { get; set; } = "Hello, World! from OverviewChartViewModel!";
        public ObservableCollection<ISeries> SalesByCategory { get; private set; } = new ObservableCollection<ISeries>();
        public ObservableCollection<Category> Categories => _categoryStore.Categories;
        public ObservableCollection<CategoryChartViewModel> CategoryChartData = new ObservableCollection<CategoryChartViewModel>();

        public OverviewChartViewModel(ICategoryStore categoryStore)
        {
            _categoryStore = categoryStore;

            Categories.CollectionChanged += OnCategoriesChanged;

            LoadChart(Categories);
        }

        private void OnCategoriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LoadChart(Categories);
        }

        private void LoadChart(IEnumerable<Category> categories)
        {
            CategoryChartData.Clear();
            SalesByCategory.Clear();

            var chartData = categories.Select(c => new CategoryChartViewModel()
            {
                Name = c.Name,
                ProductCount = (double)c.Products
                    .Where(p => p.Purchase > 0)
                    .Sum(x => x.NetRetail)
            });

            foreach (var item in chartData)
            {
                CategoryChartData.Add(item);
            }

            //var pieSeries = CategoryChartData.Select(c => new PieSeries<double>()
            //{
            //    Values = new double[] { c.ProductCount },
            //    InnerRadius = 50
            //});

            //foreach (var item in pieSeries)
            //{
            //    SalesByCategory.Add(item);
            //}

            //SalesByCategory.Add(new PieSeries<CategoryChartViewModel>
            //{
            //    Values = CategoryChartData,
            //    Mapping = (instance, index) =>
            //    {
            //        return new Coordinate(index, instance)
            //    }
            //});
        }
    }
}

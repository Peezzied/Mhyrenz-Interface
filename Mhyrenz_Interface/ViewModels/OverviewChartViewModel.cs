using LiveCharts;
using LiveCharts.Wpf;
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
        public int ProductCount { get; set; }
    }

    public class OverviewChartViewModel: BaseViewModel
    {
        private readonly ICategoryStore _categoryStore;
        public string Bindtest { get; set; } = "Hello, World! from OverviewChartViewModel!";
        public SeriesCollection SalesByCategory { get; } = new SeriesCollection();
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
                ProductCount = c.Products
                    .Where(p => p.Purchase > 0)
                    .Sum(x => x.Purchase)
            });

            foreach (var item in chartData)
            {
                CategoryChartData.Add(item);
            }

            var pieSeries = CategoryChartData.Select(c => new PieSeries()
            {
                Title = c.Name,
                Values = new ChartValues<int> { c.ProductCount },
                DataLabels = true
            });

            foreach (var item in pieSeries)
            {
                SalesByCategory.Add(item);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Mhyrenz_Interface.Domain.Services.ProductService;
using NUnit.Framework;

namespace Mhyrenz_Interface.Test
{
    [TestFixture]
    public class RepositoryTest
    {
        InventoryDbService _db;

        IProductDataService _productDataService;
        IProductService _productService;

        ICategoryDataService _categoryDataService;
        ICategoryService _categoryService;

        [SetUp]
        public void Setup()
        {
            _db = new InventoryDbService(@"D:\General Project Bins\Mhyrenz Interface\Mhyrenz_Interface\bin\Debug\dev_inventory.db");

            _productDataService = new ProductDataService(_db);
            _productService = new ProductService(_productDataService);

            _categoryDataService = new CategoryDataService(_db);
            _categoryService = new CategoryService(_categoryDataService);
        }

        [Test]
        public void TableNameTest_Products()
        {
            Assert.That(typeof(Product).TableName(), Is.EqualTo("Products"));
        }
        [Test]
        public void TableNameTest_Categories()
        {
            Assert.That(typeof(Category).TableName(), Is.EqualTo("Categories"));
        }

        [Test]
        public async Task ProductDataServiceTest_GetAll()
        {
            var products = await _productDataService.GetAll();

            Assert.That(products.Count(), Is.GreaterThan(0));
        }

        [Test]
        public async Task ProductDataServiceTest_GetAllWithIgnore()
        {
            var products = await _productDataService.GetAllWithIgnore();

            Assert.That(products.Count(), Is.GreaterThan(0));
        }

        [Test]
        public async Task ProductServiceTest_GetAll()
        {
            var products = await _productService.GetAll();

            Assert.That(products.Count(), Is.GreaterThan(0));
        }

        [Test]
        public async Task CategoryServiceTest_Get()
        {
            var categories = await _categoryService.GetAllCategories();
            Assert.That(categories.Count, Is.GreaterThan(0));
        }

        [Test]
        public async Task ProductServiceTest_Get_499()
        {
            var product = await _productService.Get(499);
            Assert.That(product.Transactions.Count, Is.GreaterThan(0));
        }
    }
}

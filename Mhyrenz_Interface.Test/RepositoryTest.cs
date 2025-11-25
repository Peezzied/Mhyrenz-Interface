using System.Linq;
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
    [SetUpFixture]
    public class RepositoryTestSetup
    {
        public static IProductDataService ProductDataService;
        public static ICategoryDataService CategoryDataService;
        public static ITransactionsDataService TransactionsDataService;

        public static IProductService ProductService;
        public static ICategoryService CategoryService;
        public static ITransactionsService TransactionsService;

        private static InventoryDbService _db;

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            // Initialize your database once
            _db = new InventoryDbService(@"D:\General Project Bins\Mhyrenz Interface\Mhyrenz_Interface\bin\Debug\dev_inventory.db");

            // Initialize DataServices
            TransactionsDataService = new TransactionsDataService(_db);
            CategoryDataService = new CategoryDataService(_db, TransactionsDataService);
            ProductDataService = new ProductDataService(_db, CategoryDataService, TransactionsDataService);

            // Initialize Services
            ProductService = new ProductService(ProductDataService);
            CategoryService = new CategoryService(CategoryDataService);
            TransactionsService = null; // or new TransactionsService(TransactionsDataService)
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            _db?.Dispose();
        }
    }

    [TestFixture]
    public class RepositoryTest
    {
        [TestFixture]
        public class TableNameTest_Pluraliztion
        {
            [Test]
            public void Products()
            {
                Assert.That(typeof(Product).TableName(), Is.EqualTo("Products"));
            }

            [Test]
            public void Categories()
            {
                Assert.That(typeof(Category).TableName(), Is.EqualTo("Categories"));
            }

            [Test]
            public void Sessions()
            {
                Assert.That(typeof(Session).TableName(), Is.EqualTo("Sessions"));
            }

            [Test]
            public void Transactions()
            {
                Assert.That(typeof(Transaction).TableName(), Is.EqualTo("Transactions"));
            }

        }

        [TestFixture]
        public class ProductNavigationTest
        {
            [Test]
            public async Task ProductDataServiceTest_GetOne_Navigation_Category()
            {
                var products = RepositoryTestSetup.ProductDataService.GetAll().First();

                Assert.That(products.Category, Is.Not.Null);
            }
        }

        [Test]
        public async Task ProductDataServiceTest_GetAll()
        {
            var products = RepositoryTestSetup.ProductDataService.GetAll();

            Assert.That(products.Count(), Is.GreaterThan(0));
        }



        [Test]
        public async Task ProductDataServiceTest_GetAllWithIgnore()
        {
            var products = RepositoryTestSetup.ProductDataService.GetAllWithIgnore();

            Assert.That(products.Count(), Is.GreaterThan(0));
        }

        [Test]
        public async Task ProductServiceTest_GetAll()
        {
            var products = await RepositoryTestSetup.ProductService.GetAll();

            Assert.That(products.Count(), Is.GreaterThan(0));
        }

        [Test]
        public async Task CategoryServiceTest_GetAll()
        {
            var categories = await RepositoryTestSetup.CategoryService.GetAllCategories();
            Assert.That(categories.Count, Is.GreaterThan(0));
        }

        [Test]
        public async Task CategoryServiceTest_Get()
        {
            var category = await RepositoryTestSetup.CategoryService.Get(1);
            Assert.That(category, Is.Not.Null);
        }

        [Test]
        public async Task ProductServiceTest_Get_499()
        {
            var product = await RepositoryTestSetup.ProductService.Get(499);
            Assert.That(product.Transactions.Count, Is.GreaterThan(0));
        }
    }
}

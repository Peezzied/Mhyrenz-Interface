using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.ProductService;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Mhyrenz_Interface.Test
{
    [TestFixture]
    public class ProductServiceTests: DatabaseTest
    {
        private ProductService _service;

        protected override void OnSetup()
        {
            _service = new ProductService(Factory);
        }

        [Test]
        public async Task Should_IsBarcodeUnique()
        {
            var product = await _service.Create(NewProduct("Barcode"));

            var eval = await _service.IsBarcodeUnique(product.Barcode);

            Assert.That(eval, Is.False);
        }

        [Test]
        public async Task Create_Should_Add_Product()
        {
            // Arrange
            var product = NewProduct("Biogesic");

            // Act
            var result = await _service.Create(product);

            // Assert
            Assert.That(result.Id, Is.GreaterThan(0));

            using (var context = Factory.CreateDbContext())
            {
                var saved = await context.Products.FindAsync(result.Id);

                Assert.That(saved, Is.Not.Null);
                Assert.That(saved.Name, Is.EqualTo("Biogesic"));
            }
        }

        [Test]
        public async Task Get_Should_Return_Product_By_Id()
        {
            // Arrange
            var product = await SeedProduct("Paracetamol");

            // Act
            var result = await _service.Get(product.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(product.Id));
            Assert.That(result.Name, Is.EqualTo("Paracetamol"));
        }

        [Test]
        public async Task GetAll_Should_Return_All_Products()
        {
            // Act
            var result = await _service.GetAll();

            // Assert
            Assert.That(result.Count, Is.Not.EqualTo(0));
        }

        [Test]
        public async Task RemovePhysical_Should_Delete_Soft_Deleted_Products()
        {
            // Arrange
            var product = await SeedProduct("Deleted Product");
            var productId = product.Id;

            await _service.RemoveMany(new[]
            {
                productId
            });

            using (var context = Factory.CreateDbContext())
            {
                var softDeleted = await context.Products
                    .IgnoreQueryFilters()
                    .FirstAsync(p => p.Id == productId);

                Assert.That(softDeleted.IsDeleted, Is.True);
            }

            // Act
            var deletedCount = _service.RemovePhysically();

            // Assert
            using (var context = Factory.CreateDbContext())
            {
                var exists = await context.Products
                    .IgnoreQueryFilters()
                    .AnyAsync(p => p.Id == productId);

                Assert.That(exists, Is.False);
            }

            Assert.That(deletedCount, Is.GreaterThanOrEqualTo(1));
        }

        private async Task<Product> SeedProduct(string name)
        {
            using (var context = Factory.CreateDbContext())
            {
                var product = NewProduct(name);

                context.Products.Add(product);
                await context.SaveChangesAsync();

                return product;
            }
        }

        private Product NewProduct(string name)
        {
            return new Product
            {
                Name = name,
                Qty = 10,
                CategoryId = 1,
                MarkupRate = 1,
                RetailPrice = 50m,
                CostPrice = 45m,
                Barcode = Guid.NewGuid().ToString("N")
            };
        }
    }
}

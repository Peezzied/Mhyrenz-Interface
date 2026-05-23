using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.SalesRecordService;
using Mhyrenz_Interface.Domain.Services.TransactionService;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Mhyrenz_Interface.Test
{
    [TestFixture]
    public class CheckoutServiceTests : DatabaseTest
    {
        private ICheckoutService _service;

        protected override void OnSetup()
        {
            _service = new CheckoutService(Factory);
        }

        [Test]
        public async Task Create_Should_Create_Active_Sale()
        {
            var sessionId = await GetExistingSessionId();

            var sale = await _service.Create(sessionId);

            Assert.That(sale.Id, Is.GreaterThan(0));
            Assert.That(sale.SessionId, Is.EqualTo(sessionId));
            Assert.That(sale.Completed_at, Is.Null);
        }

        [Test]
        public async Task AddItem_WithSale_Should_Create_Transaction()
        {
            var sessionId = await GetExistingSessionId();
            var sale = await _service.Create(sessionId);
            var productId = await GetExistingProductId();

            var discount = new DiscountInfo
            {
                Discount = Discount.None,
                DiscountRate = 0m
            };

            var result = await _service.AddItem(
                sale.Id,
                productId,
                discount,
                amount: 2);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Sale.Id, Is.EqualTo(sale.Id));
            Assert.That(result.Transaction, Is.Not.Null);
            Assert.That(result.Transaction.ProductId, Is.EqualTo(productId));
            Assert.That(result.Transaction.Amount, Is.EqualTo(2));
            Assert.That(result.Transaction.Item, Is.Not.Null);
        }

        [Test]
        public async Task AddItem_WithSameProductAndDiscount_Should_Increase_Existing_Transaction()
        {
            var sessionId = await GetExistingSessionId();
            var sale = await _service.Create(sessionId);
            var productId = await GetExistingProductId();

            var discount = new DiscountInfo
            {
                Discount = Discount.None,
                DiscountRate = 0m
            };

            await _service.AddItem(sale.Id, productId, discount, amount: 2);
            var result = await _service.AddItem(sale.Id, productId, discount, amount: 3);

            Assert.That(result.Transaction.Amount, Is.EqualTo(5));

            using (var context = Factory.CreateDbContext())
            {
                var transactionCount = await context.Transactions
                    .CountAsync(t => t.SaleId == sale.Id && t.ProductId == productId);

                Assert.That(transactionCount, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task AddItem_WithDifferentDiscount_Should_Create_New_Transaction()
        {
            var sessionId = await GetExistingSessionId();
            var sale = await _service.Create(sessionId);
            var productId = await GetExistingProductId();

            await _service.AddItem(
                sale.Id,
                productId,
                new DiscountInfo
                {
                    Discount = Discount.None,
                    DiscountRate = 0m
                },
                amount: 1);

            await _service.AddItem(
                sale.Id,
                productId,
                new DiscountInfo
                {
                    Discount = Discount.Custom,
                    DiscountRate = 0.10m
                },
                amount: 1);

            using (var context = Factory.CreateDbContext())
            {
                var transactionCount = await context.Transactions
                    .CountAsync(t => t.SaleId == sale.Id && t.ProductId == productId);

                Assert.That(transactionCount, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task Subtract_WithSale_Should_Decrease_Transaction_Amount()
        {
            var sessionId = await GetExistingSessionId();
            var sale = await _service.Create(sessionId);
            var productId = await GetExistingProductId();

            var added = await _service.AddItem(
                sale.Id,
                productId,
                new DiscountInfo
                {
                    Discount = Discount.None,
                    DiscountRate = 0m
                },
                amount: 5);

            var result = await _service.Subtract(
                sale.Id,
                added.Transaction.Id,
                amount: 2);

            Assert.That(result.Transaction, Is.Not.Null);
            Assert.That(result.Transaction.Amount, Is.EqualTo(3));
        }

        [Test]
        public async Task Subtract_ToZero_Should_Remove_Transaction()
        {
            var sessionId = await GetExistingSessionId();
            var sale = await _service.Create(sessionId);
            var productId = await GetExistingProductId();

            var added = await _service.AddItem(
                sale.Id,
                productId,
                new DiscountInfo
                {
                    Discount = Discount.None,
                    DiscountRate = 0m
                },
                amount: 2);

            var result = await _service.Subtract(
                sale.Id,
                added.Transaction.Id,
                amount: 2);

            Assert.That(result.Transaction, Is.Null);

            using (var context = Factory.CreateDbContext())
            {
                var exists = await context.Transactions
                    .AnyAsync(t => t.Id == added.Transaction.Id);

                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public async Task AddItem_DirectProduct_Should_Create_Inventory_Transaction()
        {
            var productId = await GetExistingProductId();

            var result = await _service.AddItem(productId, amount: 3);

            Assert.That(result.Id, Is.EqualTo(productId));
            Assert.That(result.Purchase, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public async Task Subtract_DirectProduct_Should_Create_Negative_Inventory_Transaction()
        {
            var productId = await GetExistingProductId();

            var result = await _service.Subtract(productId, amount: 2);

            Assert.That(result.Transactions.Count, Is.EqualTo(1));
            Assert.That(result.Id, Is.EqualTo(productId));
        }

        private async Task<int> GetExistingProductId()
        {
            using (var context = Factory.CreateDbContext())
            {
                return await context.Products
                    .Where(p => !p.IsDeleted)
                    .Select(p => p.Id)
                    .FirstAsync();
            }
        }

        private async Task<Guid> GetExistingSessionId()
        {
            using (var context = Factory.CreateDbContext())
            {
                var existingSessionId = await context.Sessions
                    .Select(s => (Guid?)s.Id)
                    .FirstOrDefaultAsync();

                if (existingSessionId.HasValue)
                {
                    return existingSessionId.Value;
                }

                var session = new Session
                {
                    Id = Guid.NewGuid()
                };

                context.Sessions.Add(session);
                await context.SaveChangesAsync();

                return session.Id;
            }
        }

        [Test]
        public async Task GetActive_Should_Return_Only_Incomplete_Sales()
        {
            var sessionId = await GetExistingSessionId();

            var activeSale = await _service.Create(sessionId);
            var completedSale = await _service.Create(sessionId);

            var productId = await GetExistingProductId();

            await _service.AddItem(
                completedSale.Id,
                productId,
                new DiscountInfo
                {
                    Discount = Discount.None,
                    DiscountRate = 0m
                },
                amount: 1);

            await _service.CompleteSale(completedSale.Id);

            var result = await _service.GetActive();

            Assert.That(result.Any(s => s.Id == activeSale.Id), Is.True);
            Assert.That(result.Any(s => s.Id == completedSale.Id), Is.False);
            Assert.That(result.All(s => s.Completed_at == null), Is.True);
        }

        [Test]
        public async Task CompleteSale_Should_Set_CompletedAt()
        {
            var sessionId = await GetExistingSessionId();
            var sale = await _service.Create(sessionId);
            var productId = await GetExistingProductId();

            await _service.AddItem(
                sale.Id,
                productId,
                new DiscountInfo
                {
                    Discount = Discount.None,
                    DiscountRate = 0m
                },
                amount: 1);

            var result = await _service.CompleteSale(sale.Id);

            Assert.That(result.Completed_at, Is.Not.Null);

            using (var context = Factory.CreateDbContext())
            {
                var savedSale = await context.Sales
                    .FirstAsync(s => s.Id == sale.Id);

                Assert.That(savedSale.Completed_at, Is.Not.Null);
            }
        }

        [Test]
        public async Task DiscardSale_Should_Delete_Sale_And_Transactions()
        {
            var sessionId = await GetExistingSessionId();
            var sale = await _service.Create(sessionId);
            var productId = await GetExistingProductId();

            var added = await _service.AddItem(
                sale.Id,
                productId,
                new DiscountInfo
                {
                    Discount = Discount.None,
                    DiscountRate = 0m
                },
                amount: 2);

            var transactionId = added.Transaction.Id;

            await _service.DiscardSale(sale.Id);

            using (var context = Factory.CreateDbContext())
            {
                var saleExists = await context.Sales
                    .AnyAsync(s => s.Id == sale.Id);

                var transactionExists = await context.Transactions
                    .AnyAsync(t => t.Id == transactionId);

                Assert.That(saleExists, Is.False);
                Assert.That(transactionExists, Is.False);
            }
        }
    }
}

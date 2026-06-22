using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Database.Services
{
    public class OrderService : IOrderService
    {
        private readonly InventoryDbContextFactory _inventoryDbContextFactory;
        private readonly ITelegramBotService _telegramBotService;

        public OrderService(InventoryDbContextFactory inventoryDbContextFactory, ITelegramBotService telegramBotService)
        {
            _inventoryDbContextFactory = inventoryDbContextFactory;
            _telegramBotService = telegramBotService;
        }

        public async Task<Order> AddItem(int productId, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var order = await context.Orders
                    .Include(x => x.Product)
                    .FirstOrDefaultAsync(o => o.ProductId == productId);

                if (order == null)
                {
                    order = new Order
                    {
                        ProductId = productId,
                        Qty = amount
                    };
                    await context.AddAsync(order);

                    await context.Entry(order)
                        .Reference(x => x.Product)
                        .LoadAsync();
                }
                else
                {
                    order.IncrementQty(amount);
                }

                await context.SaveChangesAsync();

                return order;
            }
        }

        public async Task<IReadOnlyList<Order>> GetOrders()
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                return await context.Orders
                    .AsNoTracking()
                    .Include(o => o.Product)
                    .ToListAsync();
            }
        }

        public async Task<Order> SubtractItem(int productId, int amount = 1)
        {
            using (var context = _inventoryDbContextFactory.CreateDbContext())
            {
                var order = await context.Orders
                    .Include(x => x.Product)
                    .FirstOrDefaultAsync(o => o.ProductId == productId) ?? throw new InvalidOperationException("Order not found.");

                order.DecrementQty(amount);

                if (order.Qty <= 0)
                {
                    context.Orders.Remove(order);
                    order = null;
                }

                await context.SaveChangesAsync();

                return order;
            }
        }

        public async Task GenerateEmail(string supplier)
        {
            var lines = await GenerateOrderLines();

            var body = $"Dear {supplier},\n\n" +
                "We would like to place an order for the following items:\n\n" +
                string.Join("\n", lines) +
                "\n\nPlease confirm availability and updated pricing.\n\n" +
                "Thank you,\n" +
                "Mhyrenz Pharmacy";

            body = Uri.EscapeDataString(body);

            Process.Start(new ProcessStartInfo
            {
                FileName = $"mailto:?body={body}",
                UseShellExecute = true
            });
        }

        public async Task SaveOrdersMessage(string title, string supplier)
        {
            var lines = await GenerateOrderLines();

            await _telegramBotService.SendMessage($"Supplier: {supplier}\n\n" +
                string.Join("\n", lines));
        }

        private async Task<IEnumerable<string>> GenerateOrderLines()
        {
            var orders = await GetOrders();

            var lines = orders.Select((item, index) => $"{index + 1,2}. {item.Product.Name} — {item.Qty}");
            return lines;
        }


    }
}

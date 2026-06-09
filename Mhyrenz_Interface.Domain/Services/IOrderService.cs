using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface IOrderService
    {
        Task<Order> AddItem(int productId, int amount = 1);
        Task GenerateEmail(string title, string supplier);
        Task<IReadOnlyList<Order>> GetOrders();
        Task SaveOrdersMessage(string title, string supplier);
        Task<Order> SubtractItem(int productId, int amount = 1);
    }
}

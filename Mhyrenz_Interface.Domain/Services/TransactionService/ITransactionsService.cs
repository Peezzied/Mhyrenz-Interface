using Mhyrenz_Interface.Domain.Models;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ITransactionsService
    {
        Task<IEnumerable<Transaction>> GetLatests();
        Task<Product> Add(Product entity, int amount = 1, bool withRecent = false);
        Task<IEnumerable<Transaction>> Remove(Product entity, int amount = 1);
        Task<bool> RemoveAll();
        Task Clear();
    }
}
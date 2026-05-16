using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ITransactionsService
    {
        Task<IEnumerable<Transaction>> GetLatests();
        Task<Product> Add(Product entity, DateTime date, int amount = 1, bool withRecent = false);
        Task<IEnumerable<Transaction>> Subtract(Product entity, int amount = 1);
        Task RemoveAll();
        Task Clear();
    }
}
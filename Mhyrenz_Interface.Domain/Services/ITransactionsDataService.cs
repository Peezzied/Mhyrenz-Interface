using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ITransactionsDataService : IWriteManyDataService<Transaction>, IReadDataService<Transaction, int>
    {
        Task<IReadOnlyList<Transaction>> GetAllByProduct(int productId);
        Task<Transaction> GetLast();
        Task Clean();
    }
}
using System.Collections.Generic;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ITransactionsDataService : IDataService<Transaction>
    {
        IEnumerable<Transaction> GetLatestsByProduct(int productId);
        IEnumerable<Transaction> GetLatests();
        Transaction GetLast();
        void Clean();
        IEnumerable<Transaction> GetAllRaw();
    }
}
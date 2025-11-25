using Mhyrenz_Interface.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ITransactionsDataService: IDataService<Transaction>
    {
        IEnumerable<Transaction> GetLatestsByProduct(int productId);
        IEnumerable<Transaction> GetLatests();
        Transaction GetLast();
        void Clean();
        IEnumerable<Transaction> GetAllRaw();
    }
}
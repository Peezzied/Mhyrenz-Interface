using Mhyrenz_Interface.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ITransactionsDataService: IDataService<Transaction>
    {
        Task<IEnumerable<Transaction>> GetLatestsByProduct(int productId);
        Task<IEnumerable<Transaction>> GetLatests();
        Task<Transaction> GetLast(); 
    }
}
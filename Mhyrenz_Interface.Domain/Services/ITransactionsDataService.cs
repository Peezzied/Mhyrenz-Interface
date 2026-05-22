using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ITransactionsDataService : IWriteManyDataService<Transaction>, IReadDataService<Transaction, int>, IWriteDataService<Transaction, int>
    {
        Task Clean();

        /// <summary>
        /// Gets a transaction for the specified product ID that is not associated with any sale.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Transaction> GetByProductId(int id);
    }
}
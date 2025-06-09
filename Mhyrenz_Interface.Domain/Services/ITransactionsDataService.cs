using Mhyrenz_Interface.Domain.Models;
using System;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ITransactionsDataService: IDataService<Transaction>
    {
        //Task<IEnumerable<Transaction>> GetAllByProduct(int productId);
        //Task<IEnumerable<Transaction>> GetAllByDateRange(DateTime startDate, DateTime endDate);
        Task<Transaction> GetLast(); 
    }
}
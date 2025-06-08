using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;

namespace Mhyrenz_Interface.Database.Services
{
    public interface ITransactionsDataService: IDataService<Transaction>
    {
        //Task<IEnumerable<Transaction>> GetAllByProduct(int productId);
        //Task<IEnumerable<Transaction>> GetAllByDateRange(DateTime startDate, DateTime endDate);
    }
}
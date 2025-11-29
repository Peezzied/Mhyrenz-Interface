using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services.SalesRecordService
{
    public interface ISalesRecordService
    {
        Task<bool> RegisterSales(SalesRecord sales);
    }
}
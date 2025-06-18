using Mhyrenz_Interface.Domain.Models;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services.SalesRecordService
{
    public interface ISalesRecordService
    {
        Task<bool> RegisterSales(SalesRecord sales);
    }
}
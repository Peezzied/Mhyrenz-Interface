using System.Collections.Generic;
using System.Windows.Threading;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services.ReportsService
{
    public interface IReportService
    {
        void Export(IEnumerable<Product> allProducts, Session session, Dispatcher dispatcher);
    }
}
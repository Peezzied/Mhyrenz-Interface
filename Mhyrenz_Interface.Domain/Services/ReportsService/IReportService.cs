using Mhyrenz_Interface.Domain.Models;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Mhyrenz_Interface.Domain.Services.ReportsService
{
    public interface IReportService
    {
        void Export(IEnumerable<Product> allProducts, Session session, Dispatcher dispatcher);
    }
}
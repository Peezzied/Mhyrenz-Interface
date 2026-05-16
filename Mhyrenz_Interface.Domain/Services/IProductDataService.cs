using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface IProductDataService : IWriteDataService<Product, int>, IWriteManyDataService<Product>, IReadDataService<Product, int>
    {
        Task<IReadOnlyList<Product>> GetAllWithIgnore();
        Task<Product> MarkChanged(Product product);
        Task<IReadOnlyList<Product>> MarkChangedRange(IEnumerable<Product> products);
    }
}
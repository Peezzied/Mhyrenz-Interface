using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services.ProductService
{
    // TODO: update to abstract the soft delete of an item.
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAll(bool ignoreFilter = false);
        Task<Product> Get(int id);
        Task<Product> Add(Product entity);
        Task Remove(Product entity);
        Task<Product> EditProperty(int id, UpdateEntity<Product> update);
        Task RemoveMany(IEnumerable<Product> products);
        Task<IEnumerable<Product>> AddMany(IEnumerable<Product> entities);
        Task<IEnumerable<Product>> EditPropertyRange(IEnumerable<Product> products, UpdateEntity<Product> update);
        Task<int> RemovePhysical();
    }
}
using Mhyrenz_Interface.Domain.Models;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services.ProductService
{
    public interface IProductService
    {
        Task <IEnumerable<Product>> GetAll();
        Task<Product> Get(int id);
        Task<Product> Add(Product entity);
        Task<bool> Remove(Product entity);
        Task<Product> EditProperty(int id, string propertyName, object value);
        Task RemoveMany(IEnumerable<Product> products);
        Task<IEnumerable<Product>> AddMany(IEnumerable<Product> entities);
        Task<IEnumerable<Product>> EditPropertyRange(IEnumerable<Product> products, string propertyName, object value);
    }
}
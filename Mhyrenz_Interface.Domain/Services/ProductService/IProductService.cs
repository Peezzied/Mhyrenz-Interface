using Mhyrenz_Interface.Domain.Models;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services.ProductService
{
    public interface IProductService
    {
        Task<Product> Add(Product entity);
        Task<bool> Remove(Product entity);
        Task<Product> Edit(int id, Product entity);
    }
}
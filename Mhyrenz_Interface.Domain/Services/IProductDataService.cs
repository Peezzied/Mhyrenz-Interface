using Mhyrenz_Interface.Domain.Models;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface IProductDataService : IDataService<Product>
    {
        Task<IEnumerable<Product>> GetAllByCategory(string name, int? id);
    }
}
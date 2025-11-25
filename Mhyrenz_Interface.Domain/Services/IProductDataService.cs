using Mhyrenz_Interface.Domain.Models;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface IProductDataService : IDataService<Product>
    {
        int DeleteAllPhysical();
        IEnumerable<Product> GetAllByCategory(string name, int? id);
        IEnumerable<Product> GetAllWithIgnore();
    }
}
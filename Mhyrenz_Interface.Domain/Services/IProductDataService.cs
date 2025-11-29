using System.Collections.Generic;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface IProductDataService : IDataService<Product>
    {
        int DeleteAllPhysical();
        IEnumerable<Product> GetAllByCategory(string name, int? id);
        IEnumerable<Product> GetAllWithIgnore();
    }
}
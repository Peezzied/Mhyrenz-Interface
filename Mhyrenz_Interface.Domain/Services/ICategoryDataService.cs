using System.Collections.Generic;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ICategoryDataService : IDataService<Category>
    {
        IEnumerable<Category> GetAllRaw();
        Category GetByName(string name);
        Category GetRaw(int id);
    }
}
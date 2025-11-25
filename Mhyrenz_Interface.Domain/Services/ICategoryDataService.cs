using Mhyrenz_Interface.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ICategoryDataService: IDataService<Category>
    {
        IEnumerable<Category> GetAllRaw();
        Category GetByName(string name);
        Category GetRaw(int id);
    }
}
using Mhyrenz_Interface.Domain.Models;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ICategoryDataService: IDataService<Category>
    {
        Task<Category> GetByName(string name);
    }
}
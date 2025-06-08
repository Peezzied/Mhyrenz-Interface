using Mhyrenz_Interface.Domain.Models;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Database.Services
{
    public interface ICategoryDataService
    {
        Task<Category> GetByName(string name);
    }
}
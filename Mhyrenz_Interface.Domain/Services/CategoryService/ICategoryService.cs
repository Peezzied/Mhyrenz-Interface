using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<Category> Get(int id);
        Task<IEnumerable<Category>> GetAllCategories();
    }
}
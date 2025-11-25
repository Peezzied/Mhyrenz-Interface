using Mhyrenz_Interface.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<Category> Get(int id);
        Task<IEnumerable<Category>> GetAllCategories();
    }
}
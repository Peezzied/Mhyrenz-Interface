using System.Collections.Generic;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ICategoryDataService : IWriteDataService<Category, int>, IReadDataService<Category, int>
    {
    }
}
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.Common;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services
{
    public interface ITransactionsService
    {
        Task<Product> Add(Product entity, int amount = 1);
        Task<Product> Remove(Product entity, int amount = 1);

    }
}
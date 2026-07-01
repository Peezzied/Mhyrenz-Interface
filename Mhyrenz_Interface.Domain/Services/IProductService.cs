using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services.ProductService
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> AddMany(IEnumerable<Product> entities);
        Task ApplyPurchases();
        Task<Product> Create(Product entity);
        Task<Product> Get(int id);
        Task<IReadOnlyList<Product>> GetAll();
        Task Update(int id, Action<Product> updater);
        Task<IReadOnlyList<Product>> RemoveMany(IEnumerable<int> productIds);
        Task<IReadOnlyList<Product>> RemoveManyBack(IEnumerable<int> productIds);
        Task RemovePhysically();
        Task<Product> SetMarkupRate(int productId, decimal rate);
        Task<bool> IsBarcodeUnique(string barcode);
        Task<int> LookupBarcode(string barcode);
    }
}
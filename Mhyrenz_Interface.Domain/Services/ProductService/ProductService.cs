using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly IProductDataService _productDataService;

        public ProductService(IProductDataService productDataService)
        {
            _productDataService = productDataService;
        }

        public async Task<Product> Add(Product entity)
        {
            return await Task.Run(() => _productDataService.Create(entity));
        }

        public async Task<IEnumerable<Product>> AddMany(IEnumerable<Product> entities)
        {
            return await Task.Run(() => _productDataService.CreateMany(entities));
        }

        public async Task<Product> EditProperty(int id, UpdateEntity<Product> update)
        {
            return await Task.Run(() =>
            {
                var newEntity = _productDataService.UpdateProperty(id, update);
                return newEntity;
            });
        }
        public async Task<IEnumerable<Product>> EditPropertyRange(IEnumerable<Product> products, UpdateEntity<Product> update)
        {
            return await Task.Run(() =>
            {
                var newEntities = _productDataService.UpdatePropertyRange(products, update);
                return newEntities;
            });
        }

        public async Task<Product> Get(int id)
        {
            return await Task.Run(() => _productDataService.Get(id) ?? throw new DataException("No product found. Please add a product first."));
        }

        public async Task<IEnumerable<Product>> GetAll(bool ignoreFilter = false)
        {
            return await Task.Run(() => { return ignoreFilter ? _productDataService.GetAllWithIgnore() : _productDataService.GetAll(); });
        }

        public async Task Remove(Product entity)
        {
            await Task.Run(() => _productDataService.Delete(entity.Id));
        }

        public async Task RemoveMany(IEnumerable<Product> products)
        {
            await Task.Run(() => _productDataService.DeleteMany(products));
        }

        public async Task<int> RemovePhysical()
        {
            return await Task.Run(() => _productDataService.DeleteAllPhysical());
        }
    }
}

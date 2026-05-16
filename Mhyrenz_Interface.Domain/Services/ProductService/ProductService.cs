using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            var product = await Get(id);
            update(product);
            return await _productDataService.MarkChanged(product);
        }

        public async Task<IEnumerable<Product>> EditPropertyRange(IEnumerable<Product> products, UpdateEntity<Product> update)
        {
            foreach (var product in products)
            {
                update(product);
            }
            return await _productDataService.MarkChangedRange(products);
        }

        public async Task<Product> Get(int id)
        {
            return await _productDataService.Get(id);
        }

        public async Task<IEnumerable<Product>> GetAll(bool ignoreFilter = false)
        {
            return ignoreFilter ? await _productDataService.GetAllWithIgnore() : await _productDataService.GetAll();
        }

        public async Task<Product> Remove(Product entity)
        {
            entity.Delete();
            return await _productDataService.MarkChanged(entity);
        }

        public async Task<IReadOnlyList<Product>> RemoveMany(IEnumerable<Product> products)
        {
            foreach (var product in products)
            {
                product.Delete();
            }
            return await _productDataService.MarkChangedRange(products);
        }

        public async Task<IReadOnlyList<Product>> RemoveManyBack(IEnumerable<Product> products)
        {
            foreach (var product in products)
            {
                product.DeleteBack();
            }
            return await _productDataService.MarkChangedRange(products);
        }

        public async Task<int> RemovePhysical()
        {
            var products = (await _productDataService.GetAllWithIgnore()).Where(x => x.IsDeleted);
            await _productDataService.DeleteMany(products);
            return products.Count();
        }
    }
}

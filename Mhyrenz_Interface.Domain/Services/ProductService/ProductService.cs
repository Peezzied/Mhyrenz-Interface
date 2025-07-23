using Mhyrenz_Interface.Domain.Exceptions;
using Mhyrenz_Interface.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return await _productDataService.Create(entity);
        }

        public async Task<IEnumerable<Product>> AddMany(IEnumerable<Product> entities)
        {
            return await _productDataService.CreateMany(entities);
        }

        public async Task<Product> EditProperty(int id, string propertyName, object value)
        {
            var newEntity = await _productDataService.UpdateProperty(id, propertyName, value) ?? throw new DataException($"Product with ID {id} not found.");
            return newEntity;
        }
        public async Task<IEnumerable<Product>> EditPropertyRange(IEnumerable<Product> products, string propertyName, object value)
        {
            var newEntities = await _productDataService.UpdatePropertyRange(products, propertyName, value);
            return newEntities;
        }

        public async Task<Product> Get(int id)
        {
            return await _productDataService.Get(id) ?? throw new DataException("No product found. Please add a product first.");
        }

        public async Task<IEnumerable<Product>> GetAll()
        {
            return await _productDataService.GetAll() ?? throw new DataException("No products found.");
        }

        public async Task<bool> Remove(Product entity)
        {
            return await _productDataService.Delete(entity.Id);
        }

        public async Task RemoveMany(IEnumerable<Product> products)
        {
            await _productDataService.DeleteMany(products);
        }
    }
}

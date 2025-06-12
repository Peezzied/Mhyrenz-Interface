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
            if (entity.ListPrice <= 0 && entity.RetailPrice <= 0)
                throw new InvalidPriceException(entity.ListPrice, entity.RetailPrice);

            await _productDataService.Create(entity);
            return entity;
        }

        public async Task<Product> Edit(int id, string propertyName, object value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            if (IsInvalidNumeric(value))
                throw new ArgumentException("Value must be greater than zero.", nameof(value));

            var newEntity = await _productDataService.UpdateProperty(id, propertyName, value) ?? throw new DataException($"Product with ID {id} not found.");
            return newEntity;
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

        private bool IsInvalidNumeric(object value)
        {
            if (value is int v4)
                return v4 <= 0;
            else if (value is long v)
                return v <= 0;
            else if (value is float v1)
                return v1 <= 0;
            else if (value is double v2)
                return v2 <= 0;
            else if (value is decimal v3)
                return v3 <= 0;

            return false;
        }
    }
}

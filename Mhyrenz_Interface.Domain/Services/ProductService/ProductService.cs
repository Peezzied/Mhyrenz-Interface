using Mhyrenz_Interface.Domain.Exceptions;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.Common;
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

        public async Task<Product> Edit(int id, Product entity)
        {
            if (entity.ListPrice <= 0 && entity.RetailPrice <= 0)
                throw new InvalidPriceException(entity.ListPrice, entity.RetailPrice);
            if (entity.Qty < 0) 
                throw new NegativeException(entity.Qty, entity);

            var newEntity = await _productDataService.Update(id, entity) ?? throw new DataException($"Product with ID {id} not found.");
            return newEntity;
        }

        public async Task<bool> Remove(Product entity)
        {
            return await _productDataService.Delete(entity.Id);
        }
    }
}

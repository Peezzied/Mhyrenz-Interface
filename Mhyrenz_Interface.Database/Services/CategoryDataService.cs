using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;

namespace Mhyrenz_Interface.Database.Services
{
    public class CategoryDataService : GenericDataService<Category>, ICategoryDataService
    {
        private readonly InventoryDbService _context;
        public CategoryDataService(InventoryDbService context) : base(context)
        {
            _context = context;
        }
        public override async Task<Category> Get(int id)
        {
            return await Task.Run(() =>
            {
                var category = GetTable().FindById(id);

                if (category != null)
                    LoadProducts(category);

                return category;
            });
        }
        public override async Task<IEnumerable<Category>> GetAll()
        {
            return await Task.Run(() =>
            {
                var list = GetTable().FindAll().ToList();

                foreach (var category in list)
                    LoadProducts(category);

                return list;
            });
        }

        public Task<Category> GetByName(string name)
        {
            throw new NotImplementedException();
        }

        // -----------------------------
        // Manual loading of relations
        // -----------------------------
        private void LoadProducts(Category category)
        {
            if (category == null) return;

            var context = _context.Instance;
            var productCol = context.GetCollection<Product>(typeof(Product).TableName());
            var trxCol = context.GetCollection<Transaction>(typeof(Transaction).TableName());

            // Load all products under this category
            category.Products = productCol.Find(p => p.CategoryId == category.Id && !p.IsDeleted).ToList();

            // Load transactions for each product
            foreach (var product in category.Products)
            {
                product.Transactions = trxCol.Query()
                    .Where(t => t.ProductId == product.Id)
                    .ToList();
            }
        }
    }
}

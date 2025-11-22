using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Database.Services
{
    public class CategoryDataService: GenericDataService<Category>, ICategoryDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;
        public CategoryDataService(InventoryDbContextFactory contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }
        public override async Task<Category> Get(int id)
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Category>(nameof(Category).TableName());
                    var category = col.FindById(id);

                    if (category != null)
                        LoadProducts(context, category);

                    return category;
                }
            });
        }
        public override async Task<IEnumerable<Category>> GetAll()
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Category>(nameof(Category).TableName());
                    var list = col.FindAll().ToList();

                    foreach (var category in list)
                        LoadProducts(context, category);

                    return list;
                }
            });
        }

        public Task<Category> GetByName(string name)
        {
            throw new NotImplementedException();
        }

        // -----------------------------
        // Manual loading of relations
        // -----------------------------
        private void LoadProducts(ILiteDatabase context, Category category)
        {
            if (category == null) return;

            var productCol = context.GetCollection<Product>(nameof(Product).TableName());
            var trxCol = context.GetCollection<Transaction>(nameof(Transaction).TableName());

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

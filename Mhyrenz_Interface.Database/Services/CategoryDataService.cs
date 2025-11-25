using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;

namespace Mhyrenz_Interface.Database.Services
{
    public class CategoryDataService : GenericDataService<Category>, ICategoryDataService
    {
        private readonly InventoryDbService _context;
        private readonly ITransactionsDataService _transactionsDataService;

        public CategoryDataService(InventoryDbService context, ITransactionsDataService transactionsDataService) : base(context)
        {
            _context = context;
            _transactionsDataService = transactionsDataService;
        }
        public new Category Get(object id)
        {
            var category = GetTable().FindById((dynamic)id);

            if (category != null)
                LoadProducts(category);

            return category;
        }
        public override IEnumerable<Category> GetAll()
        {
            var list = GetTable().FindAll().ToList();

            foreach (var category in list)
                LoadProducts(category);

            return list;
        }

        public IEnumerable<Category> GetAllRaw()
        {
            return base.GetAll();
        }

        public Category GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public Category GetRaw(int id)
        {
            return base.Get(id);
        }

        // -----------------------------
        // Manual loading of relations
        // -----------------------------
        private void LoadProducts(Category category)
        {
            if (category == null) return;

            var productCol = _context.Instance.GetCollection<Product>(typeof(Product).TableName());

            // Load all products under this category
            category.Products = productCol.Find(p => p.CategoryId == category.Id && !p.IsDeleted).ToList();

            // Load transactions for each product
            foreach (var product in category.Products)
            {
                product.Transactions = _transactionsDataService.GetAllRaw()
                    .Where(t => t.ProductId == product.Id)
                    .ToList();
            }
        }
    }
}

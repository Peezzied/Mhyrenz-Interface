using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Mhyrenz_Interface.Database.Services
{
    public class ProductDataService : GenericDataService<Product>, IProductDataService
    {
        private readonly InventoryDbContextFactory _contextFactory;

        public ProductDataService(InventoryDbContextFactory contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override async Task<Product> Get(int id)
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Product>(Name);
                    var product = col.FindById(id);

                    if (product != null && !product.IsDeleted)
                        LoadReference(context, product);

                    return product;
                }
            });

        }

        public override async Task<IEnumerable<Product>> GetAll()
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Product>(Name);
                    var list = col.Find(p => !p.IsDeleted).ToList();

                    LoadReferences(context, list);

                    return list;
                }
            });
        }

        public async Task<IEnumerable<Product>> GetAllWithIgnore()
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Product>(Name);
                    var list = col.FindAll().ToList();

                    LoadReferences(context, list);

                    return list;
                }
            });
        }

        public async Task<IEnumerable<Product>> GetAllByCategory(string name, int? id= null)
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Product>(Name);
                    var list = col.Find(p =>
                        !p.IsDeleted &&
                        (!string.IsNullOrEmpty(name) && p.Category.Name == name) &&
                        (id.HasValue && p.CategoryId == id)
                    ).ToList();

                    LoadReferences(context, list);

                    return list;
                }
            });
        }

        public async Task<int> DeleteAllPhysical()
        {
            return await Task.Run(() =>
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var col = context.GetCollection<Product>(Name);
                    var deleted = col.Find(p => p.IsDeleted).ToList();

                    foreach (var p in deleted)
                        col.Delete(p.Id);

                    return deleted.Count;
                }
            });
        }

        // -----------------------------
        // Manual relationship loading
        // -----------------------------
        private void LoadReferences(ILiteDatabase context, List<Product> list)
        {
            foreach (var product in list)
                LoadReference(context, product);
        }

        private void LoadReference(ILiteDatabase context, Product product)
        {
            if (product == null) return;

            var categoryCol = context.GetCollection<Category>(nameof(Category).TableName());
            var trxCol = context.GetCollection<Transaction>(nameof(Transaction).TableName());

            // Load category
            product.Category = categoryCol.FindById(product.CategoryId);

            // Load transactions
            product.Transactions = trxCol.Query()
                .Where(t => t.ProductId == product.Id)
                .ToList();
        }
    }
}

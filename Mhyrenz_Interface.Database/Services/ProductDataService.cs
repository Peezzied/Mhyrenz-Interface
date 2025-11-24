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
        private readonly InventoryDbService _context;

        public ProductDataService(InventoryDbService context) : base(context)
        {
            _context = context;
        }

        public override async Task<Product> Get(int id)
        {
            return await Task.Run(() =>
            {
                var product = GetTable().FindById(id);

                if (product != null && !product.IsDeleted)
                    LoadReference(product);

                return product;
            });

        }

        public override async Task<IEnumerable<Product>> GetAll()
        {
            return await Task.Run(() =>
            {
                var list = GetTable().Find(p => !p.IsDeleted).ToList();

                LoadReferences(list);

                return list;
            });
        }

        public async Task<IEnumerable<Product>> GetAllWithIgnore()
        {
            return await Task.Run(() =>
            {
                var list = GetTable().FindAll().ToList();

                LoadReferences(list);

                return list;
            });
        }

        public async Task<IEnumerable<Product>> GetAllByCategory(string name, int? id = null)
        {
            return await Task.Run(() =>
            {
                var list = GetTable().Find(p =>
                    !p.IsDeleted &&
                    (!string.IsNullOrEmpty(name) && p.Category.Name == name) &&
                    (id.HasValue && p.CategoryId == id)
                ).ToList();

                LoadReferences(list);

                return list;
            });
        }

        public async Task<int> DeleteAllPhysical()
        {
            return await Task.Run(() =>
            {
                var col = GetTable();
                var deleted = col.Find(p => p.IsDeleted).ToList();

                foreach (var p in deleted)
                    col.Delete(p.Id);

                return deleted.Count;
            });
        }

        // -----------------------------
        // Manual relationship loading
        // -----------------------------
        private void LoadReferences(List<Product> list)
        {
            foreach (var product in list)
                LoadReference(product);
        }

        private void LoadReference(Product product)
        {
            if (product == null) return;

            var context = _context.Instance;
            var categoryCol = context.GetCollection<Category>(typeof(Category).TableName());
            var trxCol = context.GetCollection<Transaction>(typeof(Transaction).TableName());

            // Load category
            product.Category = categoryCol.FindById(product.CategoryId);

            // Load transactions
            product.Transactions = trxCol.Query()
                .Where(t => t.ProductId == product.Id)
                .ToList();
        }
    }
}

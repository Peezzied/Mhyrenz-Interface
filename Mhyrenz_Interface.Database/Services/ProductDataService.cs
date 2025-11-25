using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;

namespace Mhyrenz_Interface.Database.Services
{
    public class ProductDataService : GenericDataService<Product>, IProductDataService
    {
        private readonly InventoryDbService _context;
        private readonly ICategoryDataService _categoryDataService;
        private readonly ITransactionsDataService _transactionsDataService;

        public ProductDataService(InventoryDbService context, ICategoryDataService categoryDataService, ITransactionsDataService transactionsDataService) : base(context)
        {
            _context = context;
            _categoryDataService = categoryDataService;
            _transactionsDataService = transactionsDataService;
        }

        public override Product Get(object id)
        {
            var product = GetTable().FindById((int)id);

            if (product != null && !product.IsDeleted)
            {
                product.Category = _categoryDataService.GetRaw(product.CategoryId);

                // Load transactions
                product.Transactions = _transactionsDataService.GetAllRaw()
                    .Where(t => (int)t.ProductId == (int)product.Id)
                    .ToList();
            }

            return product;
        }

        public override IEnumerable<Product> GetAll()
        {
                var list = GetTable().Find(p => !p.IsDeleted).ToList();

                LoadReferences(list);

                return list;
        }

        public IEnumerable<Product> GetAllWithIgnore()
        {
                var list = GetTable().FindAll().ToList();

                LoadReferences(list);

                return list;
        }

        public IEnumerable<Product> GetAllByCategory(string name, int? id = null)
        {
                var list = GetTable().Find(p =>
                    !p.IsDeleted &&
                    (!string.IsNullOrEmpty(name) && p.Category.Name == name) &&
                    (id.HasValue && p.CategoryId == id)
                ).ToList();

                LoadReferences(list);

                return list;
        }

        public int DeleteAllPhysical()
        {
                var col = GetTable();
                var deleted = col.Find(p => p.IsDeleted).ToList();

                foreach (var p in deleted)
                    col.Delete(p.Id);

                return deleted.Count;
        }

        // -----------------------------
        // Manual relationship loading
        // -----------------------------
        private void LoadReferences(List<Product> list)
        {
            // 1 — load categories ONCE
            var categoryIds = list.Select(p => p.CategoryId).Distinct().ToHashSet();
            var categories = _categoryDataService.GetAllRaw()
                .Where(c => categoryIds.Contains(c.Id))
                .ToDictionary(c => c.Id);

            // 2 — load transactions ONCE
            var allTransactions = _transactionsDataService.GetAllRaw();
            var trxLookup = allTransactions
                .GroupBy(t => (int)t.ProductId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 3 — map references
            foreach (var p in list)
            {
                categories.TryGetValue(p.CategoryId, out var cat);
                p.Category = cat;

                trxLookup.TryGetValue((int)p.Id, out var trx);
                p.Transactions = trx;
            }
        }
    }
}

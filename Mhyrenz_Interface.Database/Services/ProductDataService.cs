using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                Product entity = await context.Products
                    .Include(a => a.Transactions)
                    .Include(a => a.Category)
                    .FirstOrDefaultAsync((e) => e.Id == id);
                return entity;

                //var entity = await base.Get(id);

                //context.Entry(entity)
                //    .Reference(p => p.Category)
                //    .Load();

                //return entity;
            }
            
        }

        public override async Task<IEnumerable<Product>> GetAll()
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                IEnumerable<Product> entity = await context.Products
                    .Include(a => a.Transactions)
                    .Include(a => a.Category)
                    .ToListAsync();
                return entity;


                //var entity = await base.GetAll();

                //foreach (var item in entity)
                //{
                //    context.Entry(item)
                //           .Reference(p => p.Category)
                //           .Load();
                //}

                //return entity;
            }
        }

        public async Task<IEnumerable<Product>> GetAllByCategory(string name, int? id= null)
        {
            using (InventoryDbContext context = _contextFactory.CreateDbContext())
            {
                IEnumerable<Product> entity = await context.Products
                    .Include(a => a.Transactions)
                    .Include(a => a.Category)
                    .Where(p =>
                        (!string.IsNullOrEmpty(name) && p.Category.Name == name) &&
                        (id.HasValue && p.CategoryId == id.Value)
                    )
                    .ToListAsync();
                return entity;
            }
        }
    }
}

using Mhyrenz_Interface.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Database
{
    public class InventoryDbContext: DbContext
    {
        public DbSet<Product> Products { get; set; }    
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public InventoryDbContext(DbContextOptions options) : base(options) { }
    }
}

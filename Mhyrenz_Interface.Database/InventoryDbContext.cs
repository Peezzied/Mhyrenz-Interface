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
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Product> Products { get; set; }    
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PharmaDetails> PharmaDetails { get; set; }

        public InventoryDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasQueryFilter(i => !i.IsDeleted)
                .HasIndex(i => i.Barcode)
                .IsUnique();
            modelBuilder.Entity<Transaction>()
                .HasQueryFilter(i => !i.Item.IsDeleted);
        }
    }
}

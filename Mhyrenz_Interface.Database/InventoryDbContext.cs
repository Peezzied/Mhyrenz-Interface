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
        public DbSet<SalesRecord> SalesRecords { get; set; }
        public DbSet<Product> Products { get; set; }    
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public InventoryDbContext(DbContextOptions options) : base(options) { }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Product>()
        //        .Property(p => p.Purchase)
        //        .HasDefaultValue(0);

        //    // hardcoded computed columns for Product
        //    modelBuilder.Entity<Product>()
        //        .Property(p => p.NetQty)
        //        .HasComputedColumnSql("Qty - Purchase");
        //    modelBuilder.Entity<Product>()
        //        .Property(p => p.NetRetail)
        //        .HasComputedColumnSql("Purchase * RetailPrice");
        //    modelBuilder.Entity<Product>()
        //        .Property(p => p.CostPrice)
        //        .HasComputedColumnSql("Qty * RetailPrice");
        //    modelBuilder.Entity<Product>()
        //        .Property(p => p.ProfitRevenue)
        //        .HasComputedColumnSql("RetailPrice - ListPrice");
        //    modelBuilder.Entity<Product>()
        //        .Property(p => p.Profit)
        //        .HasComputedColumnSql("Purchase * ProfitRevenue");
        //    modelBuilder.Entity<Product>()
        //        .Property(p => p.TotalListPrice)
        //        .HasComputedColumnSql("ListPrice * Qty");

        //    base.OnModelCreating(modelBuilder);
        //}
    }
}

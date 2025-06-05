using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Database
{
    public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
    {
        static InventoryDbContextFactory()
        {
            SQLitePCL.Batteries.Init();
        }
        public InventoryDbContext CreateDbContext(string[] args = null)
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>();
            options.UseSqlite("Data Source=inventory.db");

            var context = new InventoryDbContext(options.Options);

            if (!IsDesignTime())
            {
                context.Database.Migrate();
            }

            return context;
        }

        // Helper to detect design-time context creation
        private static bool IsDesignTime()
        {
            return AppDomain.CurrentDomain.FriendlyName.Contains("ef");
        }
    }
}

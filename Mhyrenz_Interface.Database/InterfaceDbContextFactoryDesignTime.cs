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
    public class InterfaceDbContextFactoryDesignTime : IDesignTimeDbContextFactory<InterfaceDbContext>
    {
        static InterfaceDbContextFactoryDesignTime()
        {
            SQLitePCL.Batteries.Init();
        }
        public InterfaceDbContext CreateDbContext(string[] args = null)
        {
            var options = new DbContextOptionsBuilder<InterfaceDbContext>();
            options.UseSqlite("Data Source=dev_design_interface.db");

            var context = new InterfaceDbContext(options.Options);

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

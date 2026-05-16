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
    public class InventoryDbContextFactory
    {
        private readonly Action<DbContextOptionsBuilder> _configureDbContext;

        public InventoryDbContextFactory(Action<DbContextOptionsBuilder> configureDbContext)
        {
            _configureDbContext = configureDbContext;
        }

        public InventoryDbContext CreateDbContext()
        {
            DbContextOptionsBuilder<InventoryDbContext> options = new DbContextOptionsBuilder<InventoryDbContext>();

            options.EnableSensitiveDataLogging().EnableDetailedErrors();

            _configureDbContext(options);

            return new InventoryDbContext(options.Options);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System;

namespace Mhyrenz_Interface.Database
{
    public class InterfaceDbContextFactory
    {
        private readonly Action<DbContextOptionsBuilder> _configureDbContext;

        public InterfaceDbContextFactory(Action<DbContextOptionsBuilder> configureDbContext)
        {
            _configureDbContext = configureDbContext;
        }

        public InventoryDbContext CreateDbContext()
        {
            DbContextOptionsBuilder<InventoryDbContext> options = new DbContextOptionsBuilder<InventoryDbContext>();

            _configureDbContext(options);

            return new InventoryDbContext(options.Options);
        }
    }
}
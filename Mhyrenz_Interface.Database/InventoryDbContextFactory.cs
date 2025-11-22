using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Mhyrenz_Interface.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace Mhyrenz_Interface.Database
{
    public class InventoryDbContextFactory
    {
        private readonly string _databasePath;

        public InventoryDbContextFactory(string databasePath = "Inventory.db")
        {
            _databasePath = databasePath;
        }

        public ILiteDatabase CreateDbContext()
        {
            ILiteDatabase database = null;
            if (database == null)
            {
                database = new LiteDatabase(_databasePath);

                database.GetCollection<Product>(nameof(Product).TableName())
                    .EnsureIndex(x => x.IsDeleted);

                //_database.GetCollection<Category>(nameof(Category).TableName())
                //    .EnsureIndex(x => x.IsDeleted);
            }

            return database;
        }
    }
}

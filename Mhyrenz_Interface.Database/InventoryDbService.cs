using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class InventoryDbService
    {
        private readonly string _databasePath;
        public ILiteDatabase Instance { get; }

        public InventoryDbService(string databasePath = "Inventory.db")
        {
            _databasePath = databasePath;
            Instance = new LiteDatabase(_databasePath);


            BsonMapper.Global.Entity<Product>()
                .Ignore(x => x.NetQty)
                .Ignore(x => x.NetRetail)
                .Ignore(x => x.CostPrice)
                .Ignore(x => x.ProfitRevenue)
                .Ignore(x => x.Profit)
                .Ignore(x => x.TotalListPrice)
                .Ignore(x => x.Purchase);

            BsonMapper.Global.Entity<Session>()
                .Id(x => x.UniqueId);

            Instance.GetCollection<Product>(typeof(Product).TableName())
                .EnsureIndex(x => x.IsDeleted);

            //_database.GetCollection<Category>(nameof(Category).TableName())
            //    .EnsureIndex(x => x.IsDeleted);
        }

        public void Dispose()
        {
            Instance.Dispose();
        }
    }
}

using LiteDB;
using Mhyrenz_Interface.Domain.Models;

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
                .Ignore(x => x.Purchase)
                .Ignore(x => x.Transactions)
                .Ignore(x => x.Category);

            BsonMapper.Global.Entity<Session>()
                .Id(x => x.Id);

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

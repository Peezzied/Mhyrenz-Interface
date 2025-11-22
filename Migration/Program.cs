using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Domain.Models;
using Microsoft.Data.Sqlite;


namespace Migration
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // Migrate Categories
            var categories = new List<Category>();
            using (var sqliteConn = new SqliteConnection("Data Source=dev_inventory.db"))
            {
                sqliteConn.Open();
                using (var cmd = new SqliteCommand(@"SELECT Id, Name FROM ""Categories""", sqliteConn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString()
                        });
                    }
                }
            }

            // Migrate Products
            var products = new List<Product>();
            using (var sqliteConn = new SqliteConnection("Data Source=dev_inventory.db"))
            {
                sqliteConn.Open();
                using (var cmd = new SqliteCommand (
                    @"SELECT Id, Name, Qty, RetailPrice, ListPrice, Barcode, Expiry, Batch, IsDeleted, CategoryId 
                  FROM ""Products""", sqliteConn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime? expiry = null;
                        if (DateTime.TryParse(reader["Expiry"]?.ToString(), out var dt))
                            expiry = dt;

                        products.Add(new Product
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Qty = Convert.ToInt32(reader["Qty"]),
                            RetailPrice = Convert.ToDecimal(reader["RetailPrice"]),
                            ListPrice = Convert.ToDecimal(reader["ListPrice"]),
                            Barcode = reader["Barcode"].ToString(),
                            Expiry = expiry,
                            Batch = reader["Batch"]?.ToString(),
                            IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                            CategoryId = Convert.ToInt32(reader["CategoryId"])
                        });
                    }
                }
            }

            // Insert into LiteDB
            using (var db = new InventoryDbContextFactory().CreateDbContext())
            {
                var categoryCol = db.GetCollection<Category>("Categories");
                categoryCol.InsertBulk(categories);
                categoryCol.EnsureIndex(x => x.Name);

                var productCol = db.GetCollection<Product>("Products");
                productCol.InsertBulk(products);
                productCol.EnsureIndex(x => x.Name);
                productCol.EnsureIndex(x => x.CategoryId);
            }

            Console.WriteLine("Migration complete!");

        }
    }
}

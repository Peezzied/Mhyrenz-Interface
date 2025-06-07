using ConsoleTables;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var contextFactory = new InventoryDbContextFactory(options => 
            {
                options.UseSqlite("Data Source=dev_inventory.db");
            });

            IDataService<Category> categoryService = new GenericDataService<Category>(contextFactory);
            IDataService<Product> productService = new GenericDataService<Product>(contextFactory);
            IDataService<Transaction> transactionsService = new GenericDataService<Transaction>(contextFactory);


            //using (InventoryDbContext context = contextFactory.CreateDbContext())
            //{
            //    context.Database.Migrate();
            //}

            var categoryTable = new ConsoleTable("Name", "Id");
            DisplayEntities(
                categoryService,
                categoryTable,
                "Category",
                (table, item) => table.AddRow(item.Name, item.Id)
            ).GetAwaiter();

            var productsTable = new ConsoleTable("Name", "Id", "CategoryId");
            DisplayEntities(
                productService,
                productsTable,
                "Products",
                (table, item) => productsTable.AddRow(item.Name, item.Id, item.CategoryId)
            ).GetAwaiter();

            //var transactionsTable = new ConsoleTable("Name", "Id", "CategoryId");
            //DisplayEntities(
            //    productService,
            //    transactionsTable,
            //    "Transactions",
            //    (table, item) => transactionsTable.AddRow(item.Name, item.Id, item.CategoryId)
            //).GetAwaiter();
        }

        private static async Task DisplayEntities<T>(
            IDataService<T> dataService,
            ConsoleTable table,
            string title,
            Action<ConsoleTable, T> addRowAction)
                {
                    var items = await dataService.GetAll();
                    Console.WriteLine($"{title} in the database:");
                    foreach (var item in items)
                    {
                        addRowAction(table, item);
                    }
                    table.Write(Format.Minimal);
                }

    }


}

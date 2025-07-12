using ConsoleTables;
using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;
using Mhyrenz_Interface.Domain.Services.ReportsService;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var contextFactory = new InventoryDbContextFactory(options =>
            {
                options.UseSqlite("Data Source=dev_inventory.db");
            });

            var categoryService = new CategoryDataService(contextFactory);
            var productService = new ProductDataService(contextFactory);
            var transactionsService = new TransactionsDataService(contextFactory);

            var pathService = new CachePath();
            var exportService = new ReportService(pathService);

            var products = productService.GetAll().GetAwaiter().GetResult();


            #region "Database test"
            //productService.Update(1, new Product()
            //{
            //    Name = "Updated Product",
            //    CategoryId = 3,
            //    Qty = 10,
            //    RetailPrice = 150.0m,
            //    ListPrice = 200.0m,
            //}).GetAwaiter();

            //var categoryTable = new ConsoleTable("Name", "Id", "Products");
            //DisplayEntities(
            //    categoryService,
            //    categoryTable,
            //    "Category",
            //    (table, item) =>
            //    {
            //        string productNames = string.Join(", ", item.Products.Select(p => p.Name));
            //        table.AddRow(item.Name, item.Id, productNames);

            //        //Debug.WriteLine($"Category: {item.Name}, Id: {item.Id}, Products: {item.Products}");
            //    }
            //).GetAwaiter();

            //var transactionsTable = new ConsoleTable("Name", "Id", "Products");
            //DisplayEntities(
            //    transactionsService,
            //    transactionsTable,
            //    "Transactions",
            //    (table, item) =>
            //    {
            //        //string productNames = string.Join(", ", item.Products.Select(p => p.Name));
            //        table.AddRow(item.Id, item.ProductId, item.Item.Name);
            //    }
            //).GetAwaiter();

            //string[] productCols = {
            //    "Name", "Id", "Purchase", "Qty", "Net Qty", "CategoryId", "Category", "Retail", "List", "TOTAL COST PRICE", "PROFIT REVENUE", "TOTAL LIST PRICE"};

            //// Common row adder logic
            //Action<ConsoleTable, Product> addProductRow = (table, item) =>
            //{
            //    table.AddRow(
            //        item.Name, item.Id, item.Purchase, item.Qty, item.NetQty, item.CategoryId, item.Category?.Name,
            //        item.RetailPrice, item.ListPrice, item.CostPrice, item.ProfitRevenue, item.TotalListPrice
            //    );
            //};

            //var productsTable = new ConsoleTable(productCols);
            //DisplayEntities(
            //    productService,
            //    productsTable,
            //    "Products",
            //    addProductRow
            //).GetAwaiter();

            //var genericTable = new ConsoleTable(productCols);
            //DisplayEntities(
            //    genericTable,
            //    "Generic Table",
            //    addProductRow,
            //    () => productService.GetAllByCategory("Generic", 1)
            //).GetAwaiter();
            #endregion
        }

        #region "Database test methods"
        private static async Task DisplayEntities<T>(
            IDataService<T> dataService,
            ConsoleTable table,
            string title,
            Action<ConsoleTable, T> addRowAction,
            Func<IDataService<T>, Task<IEnumerable<T>>> data = null)
        {
            var items = data != null
                ? await data(dataService)
                : await dataService.GetAll();

            Console.WriteLine($"{title} in the database:");
            foreach (var item in items)
            {
                addRowAction(table, item);
            }
            table.Write(Format.Minimal);
        }

        private static async Task DisplayEntities<T>(
            ConsoleTable table,
            string title,
            Action<ConsoleTable, T> addRowAction,
            Func<Task<IEnumerable<T>>> data)
        {
            var items = await data();

            Console.WriteLine($"{title} in the database:");
            foreach (var item in items)
            {
                addRowAction(table, item);
            }
            table.Write(Format.Minimal);
        }
        #endregion
    }

}

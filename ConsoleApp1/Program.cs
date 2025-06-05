using Mhyrenz_Interface.Database;
using Mhyrenz_Interface.Database.Services;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services;
using System;
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

            IDataService<Product> dataService = new GenericDataService<Product>(new InventoryDbContextFactory());
            dataService.Create(new Product { Name = "Tite", Category = new Category() { Name = "MyTest" } }).Wait();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Mhyrenz_Interface.ViewModels.Fake
{
    public class FakeTransactionDataGridViewModel
    {
        public List<object> Transactions { get; set; }

        public FakeTransactionDataGridViewModel()
        {
            Transactions = new List<object>();
            Transactions.Add(new { Name = "Lorem", Price = 10, Amount = 20, Date = DateTime.Now, Product = new { CategoryName = "Branded", CategoryColor = Brushes.Red } });
            Transactions.Add(new { Name = "Lorem", Price = 10, Amount = 20, Date = DateTime.Now, Product = new { CategoryName = "Branded", CategoryColor = Brushes.Red } });
            Transactions.Add(new { Name = "Lorem", Price = 10, Amount = 20, Date = DateTime.Now, Product = new { CategoryName = "Branded", CategoryColor = Brushes.Red } });
            Transactions.Add(new { Name = "Lorem", Price = 10, Amount = 20, Date = DateTime.Now, Product = new { CategoryName = "Branded", CategoryColor = Brushes.Red } });

        }
    }
}

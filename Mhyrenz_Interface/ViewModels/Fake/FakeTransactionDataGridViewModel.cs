using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using HandyControl.Tools.Extension;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.ViewModels.Fake
{
    public class FakeTransactionDataGridViewModel
    {
        public ObservableCollection<object> Transactions { get; } =
        new ObservableCollection<object>(
            Enumerable.Range(1, 20)
                .Select(i => new
                {
                    Qty = i,
                    RetailPrice = 5m,
                    Discount = "asdasd",
                    TotalPrice = i * 5m,
                    Product = new
                    {
                        Name = $"Lorem Ipsum dolor sit amet {i}"
                    }
                }));
    }
}

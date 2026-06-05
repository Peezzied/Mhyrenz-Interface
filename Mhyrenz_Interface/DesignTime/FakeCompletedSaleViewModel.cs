using System;
using System.Collections.ObjectModel;
using System.Linq;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.DesignTime
{
    public class FakeCompletedSaleViewModel
    {
        public ObservableCollection<Sale> CompletedSales { get; }
            = new ObservableCollection<Sale>();

        public FakeCompletedSaleViewModel()
        {
            var discounts = new[]
            {
            Discount.None,
            Discount.Senior,
            Discount.PWD
        };

            var random = new Random();

            var items = Enumerable.Range(1, 12)
                .Select(i =>
                {
                    var subtotal =
                        random.Next(200, 3000);

                    var discount =
                        discounts[random.Next(discounts.Length)];

                    var discountRate =
                        discount == Discount.None
                            ? 0m
                            : 0.20m;

                    var total =
                        subtotal * (1 - discountRate);

                    var paid =
                        Math.Ceiling(total / 100m) * 100m;

                    return new Sale
                    {
                        Id = i,

                        Created_at =
                            DateTime.Now.AddMinutes(-i * 14),

                        Completed_at =
                            DateTime.Now.AddMinutes(-i * 12),

                        SubTotal = subtotal,

                        Total = total,

                        Paid = paid,

                        Discount = discount,

                        // placeholder
                        // when you add SaleName later
                        // SaleName = $"Walk-in Customer #{i}"
                    };
                });

            foreach (var item in items)
            {
                CompletedSales.Add(item);
            }
        }
    }
}

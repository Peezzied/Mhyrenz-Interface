using System.Collections.ObjectModel;
using System.Linq;

namespace Mhyrenz_Interface.DesignTime
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

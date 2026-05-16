using System;

namespace Mhyrenz_Interface.Domain.Models
{
    [Obsolete]
    public class Sale
    {
        public int ProductId { get; set; }
        public string Barcode { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Qty { get; set; }

        public Product Item { get; set; }
    }
}

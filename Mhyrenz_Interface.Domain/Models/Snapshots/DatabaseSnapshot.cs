using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models.Snapshots
{
    public class DatabaseSnapshot
    {
        public List<ProductSnapshot> Products { get; set; }
        public List<SaleSnapshot> Sales { get; set; }
        public List<TransactionSnapshot> Transactions { get; set; }
    }
}

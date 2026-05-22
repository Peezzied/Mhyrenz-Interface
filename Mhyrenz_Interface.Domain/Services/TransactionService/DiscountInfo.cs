using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;

namespace Mhyrenz_Interface.Domain.Services.TransactionService
{
    public class DiscountInfo
    {
        public Discount Discount { get; set; }
        public decimal DiscountRate { get; set; }
    }
}

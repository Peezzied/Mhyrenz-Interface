using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models.Settings
{
    public class DiscountSettings
    {
        public decimal PWD { get; set; }
        public decimal Senior { get; set; }

        public decimal GetRate(Discount discount)
        {
            decimal rate;
            switch(discount)
            {
                case Discount.PWD:
                    rate = PWD;
                    break;
                case Discount.Senior:
                    rate = Senior;
                    break;
                default:
                    rate = 0m;
                    break;
            };
            return rate;
        }
    }
}

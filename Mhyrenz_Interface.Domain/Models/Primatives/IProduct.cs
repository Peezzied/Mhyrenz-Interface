using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Models.Primatives
{
    public interface IProduct
    {
        string Name { get; set; }
        //string Supplier { get; set; } // for later
        int Qty { get; set; }
        int Barcode { get; set; }
        int Category { get; set; }
    }
}

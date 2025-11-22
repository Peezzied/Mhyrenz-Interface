using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Database
{
    public static class Utils
    {
        public static string TableName(this object value)
        {
            return value + "s";
        }
    }
}

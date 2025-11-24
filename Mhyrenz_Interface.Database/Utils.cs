using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pluralize.NET;

namespace Mhyrenz_Interface.Database
{
    public static class Utils
    {
        public static string TableName(this Type value)
        {
            return new Pluralizer().Pluralize(value.Name);
        }
    }
}

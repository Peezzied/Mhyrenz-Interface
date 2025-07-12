using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Core.ValidationAttributes
{
 
        public class FutureDateAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                if (value is DateTime dt)
                {
                    return dt > DateTime.Now.AddMinutes(1);
                }
                return false;
            }
        }

        public class MustBeFalseAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                return value is bool b && b == false;
            }
        }
}

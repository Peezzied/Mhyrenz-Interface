using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Mhyrenz_Interface.Controls.MarkupEx
{
    public class EnumValuesExtension : MarkupExtension
    {
        public Type EnumType { get; set; }

        public EnumValuesExtension()
        {
        }

        public EnumValuesExtension(Type enumType)
        {
            EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (EnumType == null)
                throw new InvalidOperationException("EnumType is required.");

            return Enum.GetValues(EnumType);
        }
    }
}

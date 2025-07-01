using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Exceptions
{
    public class InvalidBarcodeEntryException : Exception
    {
        public string Barcode { get; set; }

        public InvalidBarcodeEntryException(string barcode)
        {
            Barcode = barcode;
        }

        public InvalidBarcodeEntryException(string message, string barcode) : base(message)
        {
            Barcode = barcode;
        }

        public InvalidBarcodeEntryException(string message, Exception innerException, string barcode) : base(message, innerException)
        {
            Barcode = barcode;
        }
    }
}

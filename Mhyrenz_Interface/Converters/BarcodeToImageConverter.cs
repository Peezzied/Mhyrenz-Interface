using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using ZXing;

namespace Mhyrenz_Interface.Converters
{
    public class BarcodeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string code)
            {
                var writer = new BarcodeWriter
                {
                    Format = GuessFormat(code),
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Width = 30,
                        Height = 27,
                        Margin = 3,
                    }
                };

                try
                {
                    var bitmap = writer.Write(code);

                    using (var memory = new MemoryStream())
                    {
                        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                        memory.Position = 0;

                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = memory;
                        image.EndInit();
                        return image;
                    }
                }
                catch (Exception e)
                {
                    using (var bmp = new System.Drawing.Bitmap(200, 50))
                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    using (var ms = new MemoryStream())
                    {
                        g.Clear(System.Drawing.Color.White);
                        using (var font = new System.Drawing.Font("Arial", 16))
                        using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
                        {
                            g.DrawString("Invalid Barcode!", font, brush, new System.Drawing.PointF(10, 15));
                        }

                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ms.Position = 0;

                        var fallback = new BitmapImage();
                        fallback.BeginInit();
                        fallback.CacheOption = BitmapCacheOption.OnLoad;
                        fallback.StreamSource = ms;
                        fallback.EndInit();
                        return fallback;
                    }
                }
            }

            return null;
        }

        private BarcodeFormat GuessFormat(string input)
        {
            if (Regex.IsMatch(input, @"^\d{12}$")) return BarcodeFormat.UPC_A;
            return BarcodeFormat.EAN_13;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}

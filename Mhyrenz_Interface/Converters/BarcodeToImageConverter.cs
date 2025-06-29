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
            if (value is string code && !string.IsNullOrEmpty(code))
            {
                var writer = new BarcodeWriter
                {
                    Format = GuessFormat(code),
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Width = 30,
                        Height = 27,
                        Margin = 4
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
                    using (var bmp = new System.Drawing.Bitmap(200, 50, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    using (var ms = new MemoryStream())
                    {
                        // Clear with transparency
                        g.Clear(System.Drawing.Color.Transparent);

                        // 🛠 Fix blurry text: enable ClearType
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                        string text = "Invalid Barcode!";
                        float maxWidth = bmp.Width;
                        float maxHeight = bmp.Height;

                        float fontSize = 32f;
                        System.Drawing.Font font;
                        System.Drawing.SizeF textSize;

                        // Fit text into the bitmap
                        do
                        {
                            font = new System.Drawing.Font("Arial", fontSize);
                            textSize = g.MeasureString(text, font);
                            fontSize -= 1f;
                        }
                        while ((textSize.Width > maxWidth || textSize.Height > maxHeight) && fontSize > 1f);

                        using (font)
                        using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
                        {
                            float x = (bmp.Width - textSize.Width) / 2f;
                            float y = (bmp.Height - textSize.Height) / 2f;

                            g.DrawString(text, font, brush, x, y);
                        }

                        // Save to stream
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ms.Position = 0;

                        // Create BitmapImage from stream
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

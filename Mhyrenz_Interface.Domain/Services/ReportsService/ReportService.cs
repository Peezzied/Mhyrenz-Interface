using ClosedXML.Excel;
using ClosedXML.Report;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Threading;

namespace Mhyrenz_Interface.Domain.Services.ReportsService
{
    public class ReportService: IReportService
    {
        private readonly ICachePath _cachePath;

        public ReportService(ICachePath cachePath)
        {
            _cachePath = cachePath;
        }

        public void Export(IEnumerable<Product> allProducts, Session session, Dispatcher dispatcher)
        {
            var grouped = allProducts
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            var path = Path.Combine(_cachePath.Dir, "Inventory template.xlsx");
            var templateBytes = File.ReadAllBytes(path);
            using (var finalWorkbook = new XLWorkbook())
            {
                foreach (var entry in grouped)
                {
                    var category = entry.Key;
                    var products = entry.Value;

                    using (var templateStream = new MemoryStream(templateBytes))
                    {
                        var template = new XLTemplate(templateStream);

                        template.AddVariable("Products", products);
                        template.Generate();

                        var sheet = template.Workbook.Worksheets.First();
                        var copiedSheet = sheet.CopyTo(category.Name);

                        finalWorkbook.AddWorksheet(copiedSheet);
                    }
                }


                dispatcher.Invoke(() =>
                {
                    var dialog = new SaveFileDialog()
                    {
                        Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                        DefaultExt = ".xlsx",
                        FileName = $"Mhyrenz Product Inventory - {session.Period:D}.xlsx",
                        Title = "Save Inventory Report"

                    };
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        finalWorkbook.SaveAs(dialog.FileName);
                    }
                }, DispatcherPriority.Background);
            };

        }

    }
}

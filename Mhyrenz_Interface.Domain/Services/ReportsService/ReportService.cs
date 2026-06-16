using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using ClosedXML.Excel;
using ClosedXML.Report;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.BarcodeCacheService;

namespace Mhyrenz_Interface.Domain.Services.ReportsService
{
    public class ReportService : IReportService
    {
        private readonly string _path;

        public ReportService()
        {
            _path = "";
        }

        public void Export(IEnumerable<Product> allProducts, Session session, Dispatcher dispatcher)
        {
            var grouped = allProducts
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            var path = Path.Combine(_path, "Inventory template.xlsx");
            var templateBytes = File.ReadAllBytes(path);

            var tasks = new List<Task<(string SheetName, IXLWorksheet Sheet)>>();

            using (var templateStream = new MemoryStream(templateBytes))
            using (var finalWorkbook = new XLWorkbook())
            {
                foreach (var entry in grouped)
                {
                    var category = entry.Key;
                    var products = entry.Value;

                    tasks.Add(Task.Run(() =>
                    {
                        var template = new XLTemplate(templateStream);

                        template.AddVariable("Products", products);
                        template.Generate();

                        var sheet = template.Workbook.Worksheets.First();

                        using (var tempWorkbook = new XLWorkbook())
                        {
                            var copiedSheet = sheet.CopyTo(category.Name);
                            return (category.Name, copiedSheet);
                        }
                    }));

                }

                Task.WhenAll(tasks).Wait();

                foreach (var result in tasks.Select(t => t.Result))
                {
                    finalWorkbook.AddWorksheet(result.Sheet);
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
            }
            ;
        }

    }
}

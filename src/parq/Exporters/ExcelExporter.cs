using OfficeOpenXml;
using OfficeOpenXml.Table;
using parq.Display.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace parq.Exporters
{
    class ExcelExporter
    {
        internal void ExportAs(string path, ViewModel viewModel)
        {
            using (var pkg = Export(viewModel))
            {
                pkg.SaveAs(new System.IO.FileInfo(path));
            }
        }
        internal ExcelPackage Export(ViewModel viewModel)
        {
            return createExcelPackage(viewModel);
        }
        ExcelPackage createExcelPackage(ViewModel viewModel)
        {
            var package = new ExcelPackage();
            package.Workbook.Properties.Title = AppSettings.Instance.ExcelTitle.Value;
            package.Workbook.Properties.Author = AppSettings.Instance.ExcelAuthor.Value;
            package.Workbook.Properties.Comments = $"Parquet CreatedBy {viewModel.CreatedBy}";

            var worksheet = package.Workbook.Worksheets.Add(AppSettings.Instance.ExcelWorksheetName.Value);
            for (int i = 0; i < viewModel.Schema.Elements.Count; i++)
            {
                var element = viewModel.Schema.Elements[i];
                worksheet.Cells[1, i + 1].Value = element.Name;

                for (int j = 0; j < viewModel.Rows.Count; j++)
                {
                    var val = viewModel.Rows[j][i];
                    worksheet.Cells[j + 2, i + 1].Value = val;
                }
            }

            var tbl = worksheet.Tables.Add(new ExcelAddressBase(fromRow: 1, fromCol: 1, toRow: (int)viewModel.RowCount, toColumn: viewModel.Schema.Elements.Count), AppSettings.Instance.ExcelDataTableName.Value);
            tbl.ShowHeader = true;
            tbl.TableStyle = TableStyles.Dark1;
            tbl.ShowTotal = false;

            worksheet.Cells[1, 1, (int)viewModel.RowCount, viewModel.Schema.Elements.Count].AutoFitColumns();

            return package;
        }
    }
}

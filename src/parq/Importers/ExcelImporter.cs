using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parquet.Data;

namespace parq.Importers
{
    class ExcelImporter : IDisposable
    {
        private ExcelPackage _excelPackage;

        public ExcelImporter(string path)
        {
            _excelPackage = new ExcelPackage(new System.IO.FileInfo(path));
        }

        public bool ContainsTable(string sheetName, string tableName)
        {
            return _excelPackage.Workbook.Worksheets.First(s=>s.Name == sheetName).Tables.Any(t => t.Name == tableName);
        }

        public void Dispose()
        {
            _excelPackage.Dispose();
        }

        internal DataSet TableToDataSet(string sheetName, string tableName)
        {
            var table = _excelPackage.Workbook.Worksheets.First(s => s.Name == sheetName).Tables.First(t => t.Name == tableName);

            var types = new Dictionary<string, Type>();
            for (int i = 0; i < table.Address.Columns; i++)
            {
                for (int j = 1; j < table.Address.Rows; j++)
                {
                    var sample = table.WorkSheet.Cells[table.Address.Start.Row + j, i + 1];
                    if (sample.Value != null)
                    {
                        types.Add(table.Columns.ElementAt(i).Name, sample.Value.GetType());
                        break;
                    }
                    if (j == table.Address.Rows-1)
                    {
                        types.Add(table.Columns.ElementAt(i).Name, typeof(string));
                    }
                }
            }

            var ds = new DataSet(table.Columns.Select(c => new SchemaElement(c.Name, types[c.Name])));

            for (int i = 1; i < table.Address.Rows; i++)
            {
                List<object> rowData = new List<object>();
                for (int j = 0; j < table.Address.Columns; j++)
                {
                    var data = table.WorkSheet.Cells[table.Address.Start.Row + i, table.Address.Start.Column + j].Value;
                    rowData.Add(data);
                }
                ds.Add(new Row(rowData));
            }

            return ds;
        }
    }
}

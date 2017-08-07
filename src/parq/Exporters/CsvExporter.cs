using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using parq.Display.Models;
using System.Linq;

namespace parq.Exporters
{
    class CsvExporter
    {
        internal void ExportAs(string outputPath, ViewModel viewModel)
        {
            using (var writer = System.IO.File.CreateText(outputPath))
            {
                if (AppSettings.Instance.CsvHasHeaders.Value)
                {
                    WriteHeaders(viewModel.Columns, writer);
                }
                foreach (var row in viewModel.Rows)
                {
                    WriteLine(row, writer);
                }
            }
        }

        private void WriteLine(object[] row, StreamWriter writer)
        {
            for (int i = 0; i < row.Length; i++)
            {
                if (i > 0)
                    writer.Write(",");

                if (row[i] != null)
                {
                    var value = row[i].ToString();
                    if (value.Contains(","))
                    {
                        // Oranges, "Apples" becomes "Oranges, \"Apples\""
                        writer.Write($"\"{value.Replace("\"", "\\\"")}\"");
                    }
                    else
                    {

                        writer.Write(value);
                    }
                }                
            }
            writer.Write(Environment.NewLine);
        }

        private void WriteHeaders(IEnumerable<ColumnDetails> columns, StreamWriter writer)
        {
            for (int i = 0; i < columns.Count(); i++)
            {
                if (i > 0)
                    writer.Write(",");

                writer.Write(columns.ElementAt(i).columnName);
            }
            writer.Write(Environment.NewLine);
        }
    }
}

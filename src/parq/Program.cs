using System;
using Parquet;
using parq.Display;
using parq.Display.Views;
using Parquet.Data;
using Config.Net;
using Parquet.Formats;
using parq.Exporters;
using parq.Importers;

namespace parq
{

    class Program
    {
        static void Main(string[] args)
        {
            if (AppSettings.Instance.ShowVersion)
            {
                Console.WriteLine(GetVersionNumber(typeof(Program).AssemblyQualifiedName));
                return;
            }

            if (string.IsNullOrEmpty(AppSettings.Instance.InputFilePath))
            {
                WriteHelp("Missing argument InputFilePath");
            }
            else
            {
                var path = System.IO.Path.Combine(AppContext.BaseDirectory, AppSettings.Instance.InputFilePath);
                Verbose("Input file chosen as {0}", path);

                if (!System.IO.File.Exists(path))
                {
                    Console.Error.WriteLine("The path {0} does not exist", path);
                    return;
                }
                else if (string.Compare(AppSettings.Instance.Mode, "import", true) == 0)
                {
                    HandleImport(path);
                }
                else if (string.Compare(AppSettings.Instance.Mode, "export", true) == 0)
                {
                    HandleExport(path);
                }
                else
                {
                    // After reading the column types give a printed list of the layout of the columns 
                    var display = new DisplayController();


                    if (string.Compare(AppSettings.Instance.Mode, "interactive", true) == 0)
                    {
                        long fileLen = 0;
                        var view = new InteractiveConsoleView();
                        view.FoldRequested += (object sender, Display.Models.ConsoleFold cf) =>
                        {
                            var nextDataSet = ReadFromParquetFileOffset(path, cf.IndexStart, cf.IndexEnd - cf.IndexStart, out fileLen);
                            var updatedViewModel = display.Get(nextDataSet);
                            view.Update(updatedViewModel);
                        };

                        var dataSet = ReadFromParquetFileOffset(path, 0, view.GetRowCount(), out fileLen);
                        var viewModel = display.Get(dataSet);
                        view.Draw(viewModel);
                    }
                    else if (string.Compare(AppSettings.Instance.Mode, "head", true) == 0)
                    {
                        if (AppSettings.Instance.Head != null)
                        {
                            long fileLen;
                            var view = new FullConsoleView();
                            var dataSet = ReadFromParquetFileOffset(path, 0, AppSettings.Instance.Head, out fileLen);
                            var viewModel = display.Get(dataSet);
                            view.Draw(viewModel);
                        }
                        else
                        {
                            WriteHelp();
                        }
                    }
                    else if (string.Compare(AppSettings.Instance.Mode, "tail", true) == 0)
                    {
                        if (AppSettings.Instance.Tail != null)
                        {
                            long fileLen;
                            var view = new FullConsoleView();
                            var dataSet = ReadFromParquetFileTail(path, AppSettings.Instance.Tail, out fileLen);
                            var viewModel = display.Get(dataSet);
                            view.Draw(viewModel);
                        }
                        else
                        {
                            WriteHelp();
                        }
                    }
                    else
                    {
                        long fileLen = 0;
                        var dataSet = ReadFromParquetFile(path, out fileLen);
                        var viewModel = display.Get(dataSet);
                        if (string.Compare(AppSettings.Instance.Mode, "full", true) == 0)
                        {
                            new FullConsoleView().Draw(viewModel);
                        }
                        else if (string.Compare(AppSettings.Instance.Mode, "schema", true) == 0)
                        {
                            new SchemaView().Draw(viewModel);
                        }
                        else if (string.Compare(AppSettings.Instance.Mode, "rowcount", true) == 0)
                        {
                            new RowCountView().Draw(viewModel);
                        }
                    }

                }
            }
        }

        private static void HandleExport(string sourcePath)
        {
            if (string.IsNullOrEmpty(AppSettings.Instance.OutputFilePath))
            {
                WriteHelp("Missing argument OutputFilePath");
            }
            else
            {
                var outputPath = System.IO.Path.Combine(AppContext.BaseDirectory, AppSettings.Instance.OutputFilePath);
                Verbose("Output file chosen as {0}", outputPath);

                if (System.IO.File.Exists(outputPath) && !AppSettings.Instance.Force.Value)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine("ERROR: The path {0} already exists. Remove the file or try again with -Force=True", outputPath);
                    Console.ResetColor();
                    return;
                }

                long fileLen = 0;
                var dataSet = ReadFromParquetFile(sourcePath, out fileLen);
                var viewModel = new DisplayController().Get(dataSet);

                Console.WriteLine("Converting {0} lines", dataSet.RowCount);

                if (string.Compare(AppSettings.Instance.ExportFormat.Value, "excel", true) == 0)
                {
                    new ExcelExporter().ExportAs(outputPath, viewModel);
                }
                else if (string.Compare(AppSettings.Instance.ExportFormat.Value, "csv", true) == 0)
                {
                    new CsvExporter().ExportAs(outputPath, viewModel);
                }
            }
        }

        private static void HandleImport(string sourcePath)
        {
            if (string.IsNullOrEmpty(AppSettings.Instance.OutputFilePath))
            {
                WriteHelp("Missing argument OutputFilePath");
            }
            else
            {
                var outputPath = System.IO.Path.Combine(AppContext.BaseDirectory, AppSettings.Instance.OutputFilePath);
                Verbose("Output file chosen as {0}", outputPath);

                if (System.IO.File.Exists(outputPath) && !AppSettings.Instance.Force.Value)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine("ERROR: The path {0} already exists. Remove the file or try again with -Force=True", outputPath);
                    Console.ResetColor();
                    return;
                }

                DataSet ds = null;

                if (string.Compare(AppSettings.Instance.ImportFormat.Value, "csv", true) == 0)
                {
                    using (var csvStream = System.IO.File.OpenRead(sourcePath))
                    {
                        ds = CsvFormat.ReadToDataSet(csvStream, new CsvOptions { InferSchema = AppSettings.Instance.CsvInferSchema.Value, HasHeaders = AppSettings.Instance.CsvHasHeaders.Value });
                    }
                }
                else if (string.Compare(AppSettings.Instance.ImportFormat.Value, "excel", true) == 0)
                {
                    using (var importer = new ExcelImporter(sourcePath))
                    {
                        if (importer.ContainsTable(AppSettings.Instance.ExcelWorksheetName.Value, AppSettings.Instance.ExcelDataTableName.Value))
                        {
                            ds = importer.TableToDataSet(AppSettings.Instance.ExcelWorksheetName.Value, AppSettings.Instance.ExcelDataTableName.Value);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Error.WriteLine("ERROR: The Input Excel spreadsheet doesn't contain a table {0} on sheet {1}", AppSettings.Instance.ExcelWorksheetName.Value, AppSettings.Instance.ExcelDataTableName.Value);
                            Console.ResetColor();
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR: only CSV or Excel are supported import formats");
                    Console.ResetColor();
                    return;
                }

                Console.WriteLine("Converting {0} lines", ds.RowCount);

                using (System.IO.Stream fileStream = System.IO.File.OpenWrite(outputPath))
                {
                    using (var writer = new ParquetWriter(fileStream))
                    {
                        writer.Write(ds);
                    }
                }
            }
        }

        private static string GetVersionNumber(string assemblyQualifiedName)
        {
            var fromVersion = (assemblyQualifiedName.Substring(assemblyQualifiedName.IndexOf("Version=") + 8));
            return fromVersion.Substring(0, fromVersion.IndexOf(','));
        }

        private static void Verbose(string format, params string[] path)
        {
            Console.WriteLine(format, path);
        }

        public static DataSet ReadFromParquetFileOffset(string path, long skip, long take, out long fileLen)
        {
            var fileInfo = new System.IO.FileInfo(path);
            fileLen = fileInfo.Length;
            return ParquetReader.ReadFile(path, null, new ReaderOptions() { Count = take, Offset = skip });
        }

        public static DataSet ReadFromParquetFile(string path, out long fileLen)
        {
            var fileInfo = new System.IO.FileInfo(path);
            fileLen = fileInfo.Length;
            return ParquetReader.ReadFile(path);
        }

        private static DataSet ReadFromParquetFileTail(string path, Option<int> tail, out long fileLen)
        {
            var peekedFileManifest = ReadFromParquetFileOffset(path, 0, 1, out fileLen);
            return ReadFromParquetFileOffset(path, peekedFileManifest.TotalRowCount - tail.Value, tail.Value, out fileLen);
        }

        private static void WriteHelp(string warning = null)
        {
            Console.WriteLine("parq\t\t-\tParquet File Inspector for .net");
            Console.WriteLine("Usage\t\t-\tparq.exe Mode=operation InputFilePath=[relativeStringPath] DisplayMinWidth=[10]");
            Console.WriteLine("\t\t\tOperation one of: interactive (default), full, schema, rowcount, head, tail, import, export");

            if (!string.IsNullOrEmpty(warning))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(warning);
                Console.ResetColor();
            }
        }
    }
}
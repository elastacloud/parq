using Config.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace parq
{
    class AppSettings : SettingsContainer
    {
        public readonly Option<string> InputFilePath = new Option<string>();
        public readonly Option<string> OutputFilePath = new Option<string>();
        public readonly Option<bool> Force = new Option<bool>(false);
        public readonly Option<bool> CsvInferSchema = new Option<bool>(true);
        public readonly Option<bool> CsvHasHeaders = new Option<bool>(true);

        public readonly Option<string> ExcelAuthor = new Option<string>("Generated by Parq");
        public readonly Option<string> ExcelTitle = new Option<string>("Parq export");
        public readonly Option<string> ExcelWorksheetName = new Option<string>("Sheet1");
        public readonly Option<string> ExcelDataTableName = new Option<string>("parquetData");

        public readonly Option<int> DisplayMinWidth = new Option<int>(10);

        public readonly Option<string> Mode = new Option<string>("interactive");

        public readonly Option<string> ImportFormat = new Option<string>("csv");
        public readonly Option<string> ExportFormat = new Option<string>("excel");


        public readonly Option<bool> Expanded = new Option<bool>(false);

        public readonly Option<bool> DisplayNulls = new Option<bool>(true);

        public readonly Option<bool> DisplayTypes = new Option<bool>(false);

        public readonly Option<string> TruncationIdentifier = new Option<string>("*");

        public readonly Option<bool> ShowVersion = new Option<bool>(false);

        public readonly Option<int> Head = new Option<int>();
        public readonly Option<int> Tail = new Option<int>();


        //singleton
        private static AppSettings instance;
        public static AppSettings Instance => instance ?? (instance = new AppSettings());

        protected override void OnConfigure(IConfigConfiguration configuration)
        {
            configuration.UseCommandLineArgs();
        }
    }
}

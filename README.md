# DEPRECATED

Please use the dotnet global version of parq that is maintained in the (Parquet.NET)[https://github.com/elastacloud/parquet-dotnet] repo

## Legacy docs follow
## Legacy docs follow
## Legacy docs follow

# Parq

![Logo](doc/img/parquet.net.png)

## Getting Started

### Windows 

The preferred way to install Parq on Windows is to do so from Chocolatey. To do so, simply open a command shell and execute:

```sh
choco install parq
```

If you have an old version of parq and want to upgrade to the latest, use:

```sh
choco upgrade parq
```

Our versioning is based off Milestones, so you can see the Milestones in Issues and the aligned Projects in github to see what features are in which release. It is recommend to always use the latest as we take the parquet-dotnet releases in each milestone, and these often include bugfixes and optimisations. 

For more information [see chocolatey page](https://chocolatey.org/packages/parq)

### Linux

We will be releasing a Debian package for parq soon, but for now, you must compile the assembly from source using dotnet build

## Usage

This tools gives a simple data inspector which lists out the columns found in a Parquet data set and the data values for those columns. 

To use, run ```dotnet parq.dll Mode=interactive InputFilePath=path/to/file.parquet DisplayMinWidth=10``` 

Arguments include:
* Mode (defaults to Interactive)
  * Interactive - breaks the dataset row-wise into folds and column-wise into sheets. Use the keyboard arrows to navigate the dataset
  * Full - displays all rows and columns of the dataset with no pause for user input. Limits the cell contents to the width of the header
  * Schema - displays no data, but lists all columns with their .NET type equivalent. Useful for inspecting content shape, for example when trying to build out model classes. 
  * Rowcount - displays no data, but lists the rowcount of the parquet file
  * Head - takes the first n rows of the dataset (controlled by the Head property) and displays using the Full mode
  * Tail - takes the last n rows of the dataset (controlled by the Tail property) and displays using the Full mode
  * Import - takes a non-parquet file at InputFilePath and converts to a parquet file at the OutputFilePath. See [Working with other data types](#working-with-other-data-types)
    * ImportMode (string: CSV | Excel default csv)
  * Export - takes a parquet file at the InputFilePath and converts to an output file at the OutputFilePath. See [Working with other data types](#working-with-other-data-types)
    * ExportMode (string: CSV | Excel default excel)
* InputFilePath - path to the input parquet file
* DisplayMinWidth (int: default 10) - as noted earlier, the interactive and full mode both display column contents up to the width of the header as we don't currently enumerate cell contents before beginning to draw. Setting DisplayMinWidth allows for long cell contents to be display.
* Expanded (bool: default False) - when a column value (individual cell) has a long body (longer than the header or the DisplayMinWidth) this setting removes the truncation of the cell (1234567...) and expands the column to the max column width of the whole dataset. Since it has to scan the whole dataset to find this max column width, there is a performance impact by setting this.
* DisplayNulls (bool: default True) - when a column value is null at the parquet level, the default (false) shows whitespace for the cell value, meaning it is a blank cell. However if a null and whitespace is a meaningful distinction (i.e. null means not entered, string.Empty means entered as blank), this setting when set to true will show a [null] placeholder for null values.
* DisplayTypes (bool: default False) - when in interactive mode, display the types and whether null allowed
* TruncationIdentifer (string: default *) - in interactive mode only, if there are large columns by way of content which would wrap text based on the size of the console window (viewport), substring the cell and displays the supplied truncation identifier coloured yellow to identify as not from the original data. 
* Head (int) - controls the number of rows to take in Mode=Head
* Tail (int) - controls the number of rows to take in Mode=Tail
* OutputFilePath - path to an output file, currently only used for CSV mode

![Parq](doc/img/parq.png)

## Working with other data types

Parq supports Import and Export modes which allow interoperability with other file formats. The semantic directionality is "Import into Parquet" and "Export from Parquet". There are two primary data formats supported, CSV and Excel. Excel is supported using a Third Party Library, EPPlus.Core. 

### Csv

When working with CSV, the following options are available:

* CSVInferSchema (bool: default true) - defaults the schema when reading CSV files
* CSVHasHeaders (bool: default true) - uses the first row to give properties names 

### Excel

When working with Excel, the following options are available:

* ExcelAuthor (string: default "Generated by Parq") - set the document properties Author to this value
* ExcelTitle (string: default "Parq Export") - set the document properties Title to this value
* ExcelWorksheetName (string: default "Sheet1") - create a worksheet with this name and add the parquet data to it
* ExcelDataTableName (string: default "parquetData") - create a Table "named range" in Excel with the range of the parquet data set with this name

## Third Party Libraries

The functionality offered by Mode=Export ExportFormat=Excel is enabled by EPPlus.Core. Thanks to Vahid Nasiri https://github.com/VahidN/EPPlus.Core and the upstream EPPlus team Including JanKallman http://epplus.codeplex.com/


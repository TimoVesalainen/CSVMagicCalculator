using CommandLine;

public sealed class CommandLineOptions
{
    [Value(0, MetaName = "Input", Required = true, HelpText = "Path to input file to read from")]
    public string InputCSVFile { get; set; } = "";

    [Value(1, MetaName = "Output", Required = true, HelpText = "Path to output file to write results to")]
    public string OutputCSVFile { get; set; } = "";

    [Value(2, MetaName = "Primary data column index", Required = true, HelpText = "Zero-index position of column of primary data column")]
    public int PrimaryDataColumn { get; set; }

    [Value(3, MetaName = "Second data column indexis", HelpText = "Zero-index position of columns of secondary data columns")]
    public IEnumerable<int> SecondaryDataColumns { get; set; } = [];

    [Option('s', "skipFirstLine", Default = true, HelpText = "Skip the processing of first line of input file")]
    public bool SkipFirstLine { get; set; }

    [Option('c', "commaChar", Default = ',', HelpText = "Comma separator character")]
    public char Comma { get; set; }

    [Option('v', "printValue", Default = false, HelpText = "Print the value used for comparison in the output")]
    public bool PrintValue { get; set; }

    [Option('g', "groupBy", Default = null, HelpText = "Group rows by data as if they had the value of first in group")]
    public int? GroupBy { get; set; }
}
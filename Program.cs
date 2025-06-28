using CommandLine;

Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
var result = Parser.Default.ParseArguments<CommandLineOptions>(args);

await result.WithParsedAsync(async (arguments) =>
{
    using var input = File.ReadAllLinesAsync(arguments.InputCSVFile);
    var lines = await input;

    using var output = File.Open(arguments.OutputCSVFile, FileMode.OpenOrCreate);

    using var writer = new StreamWriter(output);
    if (arguments.SkipFirstLine)
    {
        await writer.WriteLineAsync(lines[0] + arguments.Comma + "Value");
    }
    foreach (var line in Calculation.Do(lines.Skip(arguments.SkipFirstLine ? 1 : 0), arguments.PrimaryDataColumn, [.. arguments.SecondaryDataColumns], arguments.Comma, arguments.PrintValue))
    {
        await writer.WriteLineAsync(line);
    }
    await writer.FlushAsync();
});
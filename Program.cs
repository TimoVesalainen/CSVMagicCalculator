Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
const int ckIndex = 6;
const int tcgIndex = 7;
const int mcmIndex = 8;
var inputPath = args[0];
var outputPath = args.Length > 1 ? args[1] : args[0] + ".out";

using var input = File.ReadAllLinesAsync(inputPath);
var lines = await input;

using var output = File.Open(outputPath, FileMode.OpenOrCreate);

using var writer = new StreamWriter(output);
await writer.WriteLineAsync(lines[0]);
foreach (var line in Calculation.Do(lines, ckIndex, tcgIndex, mcmIndex))
{
    await writer.WriteLineAsync(line);
}
await writer.FlushAsync();
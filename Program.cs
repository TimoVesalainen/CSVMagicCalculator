Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
const int ckIndex = 6;
const int tcgIndex = 7;
const int mcmIndex = 8;
var inputPath = args[0];
var outputPath = args.Length > 1 ? args[1] : args[0] + ".out";

using var input = File.ReadAllLinesAsync(inputPath);
var lines = await input;

var ckAmount = 0;
var ckySum = 0.0d;
var ckxSum = 0.0d;
var ckxySum = 0.0d;
var ckxxSum = 0.0d;

var tcgAmount = 0;
var tcgySum = 0.0d;
var tcgxSum = 0.0d;
var tcgxySum = 0.0d;
var tcgxxSum = 0.0d;

var tempLine = new List<string>(16);
var cellLines = new List<(string line, double mcmPrice, double ckPrice, double tcgPrice)>(lines?.Length ?? 4);
foreach (var line in lines.Skip(1))
{
    try
    {
        string[] cells;
        if (line.Contains('"'))
        {
            tempLine.Clear();
            for (int i = 0; i < line.Length;)
            {
                if (line[i] == '"')
                {
                    var indexOfEnd = line.IndexOf('"', i + 1);
                    var length = indexOfEnd - i + 1;
                    tempLine.Add(line.Substring(i, length));
                    i += length + 1;
                }
                else
                {
                    var indexOfEnd = line.IndexOf(',', i);
                    if (indexOfEnd < 0)
                    {
                        indexOfEnd = line.Length;
                    }
                    var length = indexOfEnd - i;
                    tempLine.Add(line.Substring(i, length));
                    i += length + 1;
                }

            }
            cells = [.. tempLine];
        }
        else
        {
            cells = line.Split(",");
        }

        if (cells.Length == 0)
        {
            continue;
        }

        var ckPrice = double.Parse(cells[ckIndex]);
        var tcgPrice = double.Parse(cells[tcgIndex]);
        var mcmPrice = double.Parse(cells[mcmIndex]);

        Console.WriteLine((ckPrice, tcgPrice, mcmPrice));
        if (mcmPrice != 0)
        {
            if (ckPrice != 0)
            {
                ckAmount++;
                ckySum += ckPrice;
                ckxSum += mcmPrice;
                ckxySum += ckPrice * mcmPrice;
                ckxxSum += mcmPrice * mcmPrice;
            }

            if (tcgPrice != 0)
            {
                tcgAmount++;
                tcgySum += tcgPrice;
                tcgxSum += mcmPrice;
                tcgxySum += tcgPrice * mcmPrice;
                tcgxxSum += mcmPrice * mcmPrice;
            }
        }

        cellLines.Add((line, mcmPrice, ckPrice, tcgPrice));
    }
    catch (Exception)
    {
        Console.Error.WriteLine("Line: " + line);
        throw;
    }
}

var mkmToCkCorr = (ckAmount * ckxySum - ckxSum * ckySum) / (ckAmount * ckxxSum - ckxSum * ckxSum);
var mkmToCkConst = (ckySum - mkmToCkCorr * ckxSum) / ckAmount;

var mkmToTcgCorr = (tcgAmount * tcgxySum - tcgxSum * tcgySum) / (tcgAmount * tcgxxSum - tcgxSum * tcgxSum);
var mkmToTcgConst = (tcgySum - mkmToTcgCorr * tcgxSum) / tcgAmount;

Console.WriteLine((mkmToCkCorr, mkmToCkConst, mkmToTcgCorr, mkmToTcgConst));

double ComparisonNumber((string line, double mcmPrice, double ckPrice, double tcgPrice) row)
{
    if (row.mcmPrice > 0)
    {
        return row.mcmPrice;
    }
    if (row.ckPrice > 0)
    {
        return (row.ckPrice - mkmToCkConst) / mkmToCkCorr;
    }
    if (row.tcgPrice > 0)
    {
        return (row.tcgPrice - mkmToTcgConst) / mkmToTcgCorr;
    }
    return 0;
}

var comparer = Comparer<double>.Default;
cellLines.Sort((a, b) => comparer.Compare(ComparisonNumber(b) , ComparisonNumber(a)));

using var output = File.Open(outputPath, FileMode.OpenOrCreate);

using var writer = new StreamWriter(output);
await writer.WriteLineAsync(lines[0]);
foreach (var line in cellLines)
{
    await writer.WriteLineAsync(String.Join(",", line.line) + "," + ComparisonNumber(line).ToString("N2"));
}
await writer.FlushAsync();
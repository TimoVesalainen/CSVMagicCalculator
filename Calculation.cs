public static class Calculation
{
    static readonly List<string> tempLine = new(16);
    static string[] SplitLine(string line, char comma)
    {
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
                    var indexOfEnd = line.IndexOf(comma, i);
                    if (indexOfEnd < 0)
                    {
                        indexOfEnd = line.Length;
                    }
                    var length = indexOfEnd - i;
                    tempLine.Add(line.Substring(i, length));
                    i += length + 1;
                }

            }
            return [.. tempLine];
        }
        else
        {
            return line.Split(comma);
        }
    }

    public static IEnumerable<string> Do(IEnumerable<string> lines, int ckIndex, int tcgIndex, int mcmIndex, char comma = ',', bool printValue = true)
    {
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

        var cacheSize = lines.TryGetNonEnumeratedCount(out var _cacheSize) ? _cacheSize : 4;
        var cellLines = new List<(string line, double mcmPrice, double ckPrice, double tcgPrice)>(cacheSize);
        foreach (var line in lines)
        {
            try
            {
                var cells = SplitLine(line, comma);

                if (cells.Length == 0)
                {
                    continue;
                }

                var ckPrice = double.Parse(cells[ckIndex]);
                var tcgPrice = double.Parse(cells[tcgIndex]);
                var mcmPrice = double.Parse(cells[mcmIndex]);

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
        cellLines.Sort((a, b) => comparer.Compare(ComparisonNumber(b), ComparisonNumber(a)));

        foreach (var line in cellLines)
        {
            if (printValue)
            {
                yield return line.line + comma + ComparisonNumber(line).ToString("N2");
            }
            else
            {
                yield return line.line;
            }
        }
    }
}
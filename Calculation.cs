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

    private class StatisticalData
    {
        public int Amount = 0;
        public double YSum = 0;
        public double XSum = 0;
        public double XYSum = 0;
        public double XXSum = 0;

        public StatisticalData()
        {
        }

        public void AddDataPoint(double x, double y)
        {
            Amount++;
            YSum += y;
            XSum += x;
            XYSum += y * x;
            XXSum += x * x;
        }

        public Func<double, double> GetYToXFunction()
        {
            var xToYCorrelation = (Amount * XYSum - XSum * YSum) / (Amount * XXSum - XSum * XSum);
            var xToYConstant = (YSum - xToYCorrelation * XSum) / Amount;

            return (yValue) => (yValue - xToYConstant) / xToYCorrelation;
        }
    }

    public static IEnumerable<string> Do(IEnumerable<string> lines, int primaryIndex, int[] secondaryIndicis, char comma, bool printValue)
    {
        var accumulatedData = secondaryIndicis.Select(_ => new StatisticalData()).ToArray();
        var cacheSize = lines.TryGetNonEnumeratedCount(out var _cacheSize) ? _cacheSize : 4;
        var cellLines = new List<(string line, double primaryPrice, List<double> secondaryPrices)>(cacheSize);
        foreach (var line in lines)
        {
            try
            {
                var cells = SplitLine(line, comma);

                if (cells.Length == 0)
                {
                    continue;
                }

                var havePrimaryPrice = double.TryParse(cells[primaryIndex], out var primaryPrice);

                var secondaryPrices = new List<double>(secondaryIndicis.Length);
                foreach (var (data, dataIndex) in accumulatedData.Zip(secondaryIndicis))
                {
                    if (double.TryParse(cells[dataIndex], out var secondaryPrice) && secondaryPrice > 0)
                    {
                        if (havePrimaryPrice && primaryPrice > 0)
                        {
                            // Only add valid prices as data points
                            data.AddDataPoint(primaryPrice, secondaryPrice);
                        }
                        secondaryPrices.Add(secondaryPrice);
                    }
                    else
                    {
                        secondaryPrices.Add(0);
                    }
                }
                cellLines.Add((line, primaryPrice, secondaryPrices));

            }
            catch (Exception)
            {
                Console.Error.WriteLine("Error in line: " + line);
                throw;
            }
        }
        var conversionFunctions = accumulatedData.Select(data => data.GetYToXFunction()).ToArray();

        double ComparisonNumber((string line, double primaryPrice, List<double> secondaryPrices) row)
        {
            if (row.primaryPrice > 0)
            {
                return row.primaryPrice;
            }
            foreach (var (secondaryPrice, conversion) in row.secondaryPrices.Zip(conversionFunctions))
            {
                if (secondaryPrice > 0)
                {
                    return conversion(secondaryPrice);
                }
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
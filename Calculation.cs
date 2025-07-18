public static class Calculation
{
    /// <summary>
    /// Helper to contain aggregate data
    /// </summary>
    private sealed class StatisticalData
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

    /// <summary>
    /// Calculate linear trend between primary value and secondary values,
    /// and use them to estimate primary value when it is missing
    /// </summary>
    public static IEnumerable<string> Do(IEnumerable<string> lines, int primaryIndex, int[] secondaryIndicis, char comma, bool printComparison, int? groupBy)
    {
        var accumulatedData = secondaryIndicis.Select(_ => new StatisticalData()).ToArray();
        var cacheSize = lines.TryGetNonEnumeratedCount(out var _cacheSize) ? _cacheSize : 4;
        var cellLines = new List<(string line, double primaryPrice, List<double> secondaryPrices)>(cacheSize);

        // Parse lines as CSV, parse values and accumulate data for conversions
        foreach (var line in lines)
        {
            try
            {
                var cells = CSV.SplitLine(line, comma);

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
                    }
                    secondaryPrices.Add(secondaryPrice);
                }
                cellLines.Add((line, primaryPrice, secondaryPrices));

            }
            catch (Exception)
            {
                Console.Error.WriteLine("Error in line: " + line);
                throw;
            }
        }
        // Calculate conversion functions from secondary values to primary value
        var conversionFunctions = accumulatedData.Select(data => data.GetYToXFunction()).ToArray();

        double ComparisonValue((string line, double primaryPrice, List<double> secondaryPrices) row)
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
        // Sort to descending order
        cellLines.Sort((a, b) => comparer.Compare(ComparisonValue(b), ComparisonValue(a)));

        foreach (var line in cellLines)
        {
            if (printComparison)
            {
                yield return line.line + comma + ComparisonValue(line).ToString("N2");
            }
            else
            {
                yield return line.line;
            }
        }
    }
}
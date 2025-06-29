public static class CSV
{
    static readonly List<string> tempLine = new(16);
    public static string[] SplitLine(string line, char comma)
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
}
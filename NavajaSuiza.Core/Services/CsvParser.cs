using System.Text;

namespace NavajaSuiza.Core.Services;

public static class CsvParser
{
    public static IReadOnlyList<string[]> Parse(string content)
    {
        var rows = new List<string[]>();

        if (string.IsNullOrEmpty(content))
            return rows;

        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var fieldIsEmpty = true;

        for (var i = 0; i < content.Length; i++)
        {
            var ch = content[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < content.Length && content[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(ch);
                }

                continue;
            }

            if (ch == '"' && fieldIsEmpty)
            {
                inQuotes = true;
                fieldIsEmpty = false;
                continue;
            }

            if (ch == ',')
            {
                row.Add(field.ToString());
                field.Clear();
                fieldIsEmpty = true;
                continue;
            }

            if (ch is '\r' or '\n')
            {
                if (ch == '\r' && i + 1 < content.Length && content[i + 1] == '\n')
                    i++;

                row.Add(field.ToString());
                field.Clear();

                AddRowIfNotEmpty(rows, row);
                row.Clear();
                fieldIsEmpty = true;
                continue;
            }

            field.Append(ch);
            fieldIsEmpty = false;
        }

        row.Add(field.ToString());
        AddRowIfNotEmpty(rows, row);

        return rows;
    }

    private static void AddRowIfNotEmpty(List<string[]> rows, List<string> row)
    {
        if (row.Count == 0)
            return;

        if (row.Count == 1 && string.IsNullOrEmpty(row[0]))
            return;

        rows.Add(row.ToArray());
    }
}
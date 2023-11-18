using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pdbget.Helpers;

public static class CsvHelper
{
    public static string FormatCsvRow(params object?[] fields)
    {
        return fields.FormatCsvRow();
    }

    public static string FormatCsvRow(this IEnumerable<object?> fields)
    {
        return string.Join(",", fields
            .Select(o => o?.ToString())
            .Select(o =>
            {
                if (string.IsNullOrWhiteSpace(o))
                    return null;
                if (o.Contains(','))
                    return o.Contains('"') ? $"\"{o.Replace("\"", "\"\"")}\"" : $"\"{o}\"";
                if (o.First() == '\"' || o.Last() == '\"')
                    return $"\"{o.Replace("\"", "\"\"")}\"";
                return o;
            }));
    }

    public static IEnumerable<string> FormatCsvRows(
        this IEnumerable<IEnumerable<object?>> rows,
        params IEnumerable<object?>[] headerSets)
    {
        return headerSets.Concat(rows).Select(o => o.FormatCsvRow());
    }

    public static IEnumerable<string[]> ParseCsvRows(
        this IEnumerable<string> rows,
        params int[] columnIds)
    {
        return rows.ParseCsvRows(',', columnIds);
    }

    public static IEnumerable<string[]> ParseCsvRows(
        this IEnumerable<string> rows,
        params string[] columnNames)
    {
        return rows.ParseCsvRows(',', columnNames);
    }

    public static IEnumerable<string[]> ParseCsvRows(
        this IEnumerable<string> rows,
        char delimiter,
        params int[] columnIds)
    {
        int maxIndex = columnIds.Max();

        foreach (string[]? fields in rows.Skip(1).Select(o => o.SplitCsvFields(delimiter)))
        {
            if (maxIndex >= fields.Length)
                yield return Array.Empty<string>();
            else
                yield return columnIds.Select(o => fields[o]).ToArray();
        }
    }

    public static IEnumerable<string[]> ParseCsvRows(
        this IEnumerable<string> rows,
        char delimiter,
        params string[] columnNames)
    {
        Dictionary<string, int> headers = rows.First().SplitCsvFields(delimiter).Select((o, i) => new { o, i }).ToDictionary(o => o.o, o => o.i);

        int[] colIndicies = new int[columnNames.Length];
        int maxIndex = -1;

        for (int i = 0; i < columnNames.Length; i++)
        {
            if (headers.TryGetValue(columnNames[i], out int value))
            {
                colIndicies[i] = value;
                maxIndex = Math.Max(maxIndex, colIndicies[i]);
            }
            else
            {
                throw new Exception($"Cannot find column {columnNames[i]}");
            }
        }

        foreach (string[]? fields in rows.Skip(1).Select(o => o.SplitCsvFields(delimiter)))
        {
            if (maxIndex >= fields.Length)
                yield return Array.Empty<string>();
            else
                yield return colIndicies.Select(o => fields[o]).ToArray();
        }
    }

    public static IEnumerable<Dictionary<string, string>> CsvRowsToDictionaries(this IEnumerable<string> rows, char delimiter = ',')
    {
        string[] headers = [.. rows.First().SplitCsvFields()];

        foreach (string[]? fields in rows.Skip(1).Select(o => o.SplitCsvFields(delimiter)))
        {
            yield return Enumerable.Range(0, headers.Length).ToDictionary(o => headers[o], o => fields[o]);
        }
    }

    public static string[] SplitCsvFields(this string row, char delimiter = ',')
    {
        List<string> list = [];
        StringBuilder sb = new();
        bool escape = false, quote = false;

        for (int i = 0; i <= row.Length; i++)
        {
            if (escape) // "...?
            {
                if (quote) // "..."?
                {
                    if (i == row.Length || row[i] == delimiter) // "...",
                    {
                        quote = false;
                        escape = false;
                        list.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (row[i] == '"') // "...""
                    {
                        quote = false;
                        sb.Append('"');
                    }
                    else // "..."x
                    {
                        throw new Exception($"Invalid character after quotation mark at col {i}");
                    }
                    continue;
                }
                else
                {
                    if (i == row.Length)
                    {
                        throw new Exception($"Closing quotation mark is missing at col {i}");
                    }
                    else if (row[i] == '"')
                    {
                        quote = true;
                        continue;
                    }
                }
            }
            else // ,...?
            {
                if (i == row.Length || row[i] == delimiter) // ,...,
                {
                    list.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }
                else if (row[i] == '"')
                {
                    escape = true;
                    continue;
                }
            }

            sb.Append(row[i]);
        }

        return [.. list];
    }
}

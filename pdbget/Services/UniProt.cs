using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;

namespace pdbget.Services;

public partial class UniProt
{
    public string Entry { get; }
    public string Uri => $"https://www.uniprot.org/uniprot/{Entry}";

    public UniProt(string entry)
    {
        Entry = entry;
    }

    public UniProt(string protein, string species = "HUMAN")
    {
        Entry = $"{protein}_{species}";
    }

    private string? GetHtml()
    {
        int[]? waitTime = [0, 500, 2000];
        string? html = null;

        for (int i = 0; html == null; i++)
        {
            if (waitTime[i] > 0)
            {
                //Logger.Warning($"[UniProt] Failed to get html for {Entry}, retry in {waitTime[i]}ms");
                Thread.Sleep(waitTime[i]);
            }

            try
            {
                using HttpClient wc = new();
                html = wc.GetStringAsync(Uri).Result;
            }
            catch (Exception)
            {
                if (i == waitTime.Length - 1)
                    return null;
            }
        }
        return html;
    }

    public record StructRecord
    (
        string UniProtId,
        string PdbEntry,
        string Method,
        string Resolution,
        string Chain,
        string Positions
    );

    public StructRecord[]? GetStructures()
    {
        string? html = GetHtml();

        if (html == null)
            return null;

        StructRecord[] rs = StructRecordsRegex().Matches(html)
            .Cast<Match>()
            .Select(o => new StructRecord
            (
                Entry,
                o.Groups[1].Value,
                o.Groups[2].Value,
                o.Groups[3].Value,
                o.Groups[4].Value,
                o.Groups[5].Value
            ))
            .ToArray();

        Dictionary<string, StructRecord> dict = [];
        foreach (StructRecord? r in rs)
        {
            if (!dict.TryGetValue(r.PdbEntry, out StructRecord? value))
            {
                dict[r.PdbEntry] = r;
            }
            else if (value != r)
            {
                //Logger.Warning($"[UniProt] PDB {r.PdbEntry} for {r.UniProtId} has different profile");
            }
        }

        return [.. dict.Values];
    }

    //                                                                 1pdb_entry                   2method          3resolution      4chain                    5positions
    [GeneratedRegex(@"href=""https://www.ebi.ac.uk/pdbe-srv/view/entry/(\w+)""[^<>]*>\1</a></td><td>([^<>]+)</td><td>([^<>]+)</td><td>([^<>]+)</td><td><a[^<>]+>([^<>]+)</a>")]
    private static partial Regex StructRecordsRegex();
}

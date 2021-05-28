using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace pdbget.Services
{
    public class UniProt
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
            int[]? waitTime = new int[] { 0, 500, 2000 };
            string? html = null;

            for (int i = 0; html == null; i++)
            {
                if (i == 3)
                {
                    return null;
                }

                if (i > 0 && i < 3)
                {
                    //Logger.Warning($"[UniProt] Failed to get html for {Entry}, retry in {waitTime[i]}ms");
                    Thread.Sleep(waitTime[i]);
                }

                try
                {
                    using var wc = new WebClient();
                    html = wc.DownloadString(Uri);
                }
                catch (Exception)
                {
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

            var rs = Regex.Matches(
                    html,
                    @"href=""https://www.ebi.ac.uk/pdbe-srv/view/entry/(\w+)""[^<>]*>\1</a></td><td>([^<>]+)</td><td>([^<>]+)</td><td>([^<>]+)</td><td><a[^<>]+>([^<>]+)</a>")
                //                                                     1pdb_entry                   2method          3resolution      4chain                    5positions
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

            var dict = new Dictionary<string, StructRecord>();
            foreach (var r in rs)
            {
                if (!dict.ContainsKey(r.PdbEntry))
                {
                    dict[r.PdbEntry] = r;
                }
                else if (dict[r.PdbEntry] != r)
                {
                    //Logger.Warning($"[UniProt] PDB {r.PdbEntry} for {r.UniProtId} has different profile");
                }
            }

            return dict.Values.ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace pdbget.Services;

// API reference: https://www.uniprot.org/help/api_queries
public class UniProt
{
    public string Entry { get; }
    public string Uri => $"https://rest.uniprot.org/uniprotkb/search?query={(IsReviewed != null ? $"reviewed:{IsReviewed.ToString()!.ToLower()}+AND+" : "")}{Entry}";
    public bool? IsReviewed { get; set; } = true;

    public UniProt(string entry)
    {
        Entry = entry;
    }

    public UniProt(string protein, string species = "HUMAN")
    {
        Entry = $"{protein}_{species}";
    }

    private string? GetJson()
    {
        int[]? waitTime = [0, 500, 2000];
        string? json = null;

        for (int i = 0; json == null; i++)
        {
            if (waitTime[i] > 0)
            {
                //Logger.Warning($"[UniProt] Failed to get json for {Entry}, retry in {waitTime[i]}ms");
                Thread.Sleep(waitTime[i]);
            }

            try
            {
                using HttpClient wc = new();
                json = wc.GetStringAsync(Uri).Result;
                if (string.IsNullOrEmpty(json) || json.Trim() == "{\"results\":[]}")
                    json = null;
            }
            catch (Exception)
            {
                if (i == waitTime.Length - 1)
                    return null;
            }
        }
        return json;
    }

    public record StructRecord
    (
        string UniProtId,
        string PdbEntry,
        string? Method,
        string? Resolution,
        string? Chain,
        string? Positions
    );

    public StructRecord[]? GetStructures()
    {
        string? json = GetJson();

        if (json == null)
            return null;

        Dictionary<string, StructRecord> dict = [];

        // Detect duplicates
        foreach (StructRecord? r in Parse(json))
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

        return [.. dict.Values.OrderBy(o => o.PdbEntry)];
    }

    private IEnumerable<StructRecord> Parse(string json)
    {
        string uniprotId = Entry;

        using JsonDocument doc = JsonDocument.Parse(json);
        // Access properties using the DOM API
        JsonElement root = doc.RootElement;
        JsonElement results = root.GetProperty("results");

        JsonElement[] matchedResults = results
            .EnumerateArray()
            .Where(o => o.GetProperty("uniProtkbId").GetString() == uniprotId
                || o.GetProperty("primaryAccession").GetString() == uniprotId)
            .ToArray();

        if (matchedResults.Length == 0)
        {
            //Logger.Current.LogError("No matching result for uniProt uniProt Id {uniprotId}", uniprotId);
            yield break;
        }

        JsonElement result = matchedResults[0];

        string uniprotKbId = result.GetProperty("uniProtkbId").GetString()!;
        JsonElement organism = result.GetProperty("organism");

        IEnumerable<JsonElement> pdbs = result
            .GetProperty("uniProtKBCrossReferences")
            .EnumerateArray()
            .Where(o => o.GetProperty("database").GetString() == "PDB");

        foreach (JsonElement pdb in pdbs)
        {
            string? pdbId = pdb.GetProperty("id").GetString();

            if (pdbId == null || pdbId.Length != 4)
            {
                //Logger.Current.LogError("Non PDB entry {PdbId} discovered under PDB database for {Entry}", pdbId, Entry);
                continue;
            }

            Dictionary<string, string?> props = pdb.GetProperty("properties")
                .EnumerateArray()
                .ToDictionary(p => p.GetProperty("key").GetString()!, p => p.GetProperty("value").GetString());

            props.TryGetValue("Method", out string? method);
            props.TryGetValue("Resolution", out string? resolution);
            props.TryGetValue("Chains", out string? chains);

            string? chain = null, positions = null;
            if (chains != null)
            {
                string[] fields = chains.Split('=');
                if (fields.Length == 2)
                {
                    chain = fields[0];
                    positions = fields[1];
                }
            }

            yield return new StructRecord(Entry, pdbId, method, resolution, chain, positions);
        }
    }
}

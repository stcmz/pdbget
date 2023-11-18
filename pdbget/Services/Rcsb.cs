using pdbget.Helpers;
using pdbget.PdbFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace pdbget.Services;

public class Rcsb(string entry)
{
    public string Entry { get; } = entry;
    public string Uri => $"https://files.rcsb.org/download/{Entry}";

    public void DownloadPdb(string fileName)
    {
        string? uri = $"{Uri}.pdb";

        int[]? waitTime = [0, 500, 2000];
        bool succeeded = false;

        for (int i = 0; !succeeded; i++)
        {
            if (waitTime[i] > 0)
            {
                //Logger.Warning($"[RCSB] Failed to download {Uri}, retry in {waitTime[i]}ms");
                Thread.Sleep(waitTime[i]);
            }

            try
            {
                using HttpClient wc = new();
                using Stream stream = wc.GetStreamAsync(uri).Result;
                using FileStream file = File.Create(fileName);
                stream.CopyTo(file);
                succeeded = true;
            }
            catch (Exception)
            {
                if (i == waitTime.Length - 1)
                    throw;
            }
        }
    }

    public bool SplitPdbChains(string pdbFileName, string outputDir, bool entryPrefix, bool overwrite, bool copyComments = false)
    {
        string backupPdbFileName = Path.Combine(outputDir, $"{Entry}_backup.pdb");

        string[] blockList = ["HOH", "H2O", "WATER"];

        using (PdbReader reader = new(pdbFileName))
        using (PdbWriter backupWriter = new(backupPdbFileName))
        {
            List<Record> records = [];
            Dictionary<(string, string, int), PdbWriter?> chainWriters = [];

            while (true)
            {
                Record? record = reader.ReadRecord();
                if (record == null)
                    break;

                if (record is AbstractChainedCoordinateRecord chainedRecord)
                {
                    string chainId = chainedRecord.ChainId.ToString();
                    if (char.IsLower(chainId[0]))
                        chainId = "@" + chainId;
                    if (string.IsNullOrWhiteSpace(chainId))
                        chainId = "Global";

                    string resName = chainedRecord.ResidueName ?? string.Empty;
                    int resSeq = chainedRecord.ResidueSequence;

                    if (!blockList.Contains(resName))
                    {
                        // A chain of amino acids is placed in a file
                        if (resName.TryParseAminoAcid(out _))
                        {
                            resName = "AminoAcids";
                            resSeq = 0;
                        }

                        (string chainId, string resName, int resSeq) key = (chainId, resName, resSeq);

                        if (!chainWriters.ContainsKey(key))
                        {
                            string resFileName = $"{(entryPrefix ? $"{Entry}_" : "")}{chainId}_{resName}{(resSeq == 0 ? "" : $"_{resSeq}")}.pdb";
                            string resFilePath = Path.Combine(outputDir, resFileName);

                            bool exists = File.Exists(resFilePath);

                            if (!overwrite && exists)
                            {
                                chainWriters[key] = null;
                                Logger.Warning($"[RCSB] Skipped existing fragment '{resFileName}' for '{Entry}'");
                            }
                            else
                            {
                                PdbWriter chainWriter = new(resFilePath);
                                chainWriters[key] = chainWriter;

                                foreach (Record rec in records)
                                {
                                    chainWriter.WriteRecord(rec);
                                }

                                if (exists)
                                    Logger.Warning($"[RCSB] Overwrote fragment '{resFileName}' for '{Entry}'");
                                else
                                    Logger.Info($"[RCSB] Writing fragment '{resFileName}' for '{Entry}'");
                            }
                        }

                        chainWriters[key]?.WriteRecord(record);
                    }
                }
                else if (copyComments)
                {
                    // Common records
                    records.Add(record);
                    foreach (PdbWriter? chainWriter in chainWriters.Values)
                    {
                        chainWriter?.WriteRecord(record);
                    }
                }

                backupWriter.WriteRecord(record);
            }

            backupWriter.Close();
            foreach (PdbWriter? chainWriter in chainWriters.Values)
            {
                chainWriter?.Close();
                chainWriter?.Dispose();
            }

            reader.Close();
        }

        // Validate the split
        if (!ComparePdbs(backupPdbFileName, pdbFileName))
            Logger.Warning($"[RCSB] Signature not matched in '{backupPdbFileName}'");
        else
            File.Delete(backupPdbFileName);

        return true;
    }

    private static bool ComparePdbs(string path1, string path2)
    {
        string[] newLines = File.ReadAllLines(path1);
        string[] oldLines = File.ReadAllLines(path2);

        if (newLines.Length != oldLines.Length)
            return false;

        for (int i = 0; i < newLines.Length; i++)
        {
            newLines[i] = newLines[i].TrimEnd();
            oldLines[i] = oldLines[i].TrimEnd();

            if (oldLines[i].Length != newLines[i].Length)
                return false;

            for (int j = 0; j < newLines[i].Length; j++)
                if (newLines[i][j] != oldLines[i][j] && !(newLines[i][j] == ' ' && oldLines[i][j] == '-'))
                    return false;
        }

        return true;
    }
}

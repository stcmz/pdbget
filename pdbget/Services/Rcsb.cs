using pdbget.Helpers;
using pdbget.PdbFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace pdbget.Services
{
    public class Rcsb
    {
        public string Entry { get; }
        public string Uri => $"https://files.rcsb.org/download/{Entry}";

        public Rcsb(string entry)
        {
            Entry = entry;
        }

        public void DownloadPdb(string fileName)
        {
            string? uri = $"{Uri}.pdb";

            using var wc = new WebClient();
            wc.DownloadFile(uri, fileName);
        }

        public bool SplitPdbChains(string pdbFileName, string outputDir, bool entryPrefix, bool overwrite, bool copyComments = false)
        {
            string backupPdbFileName = Path.Combine(outputDir, $"{Entry}_backup.pdb");

            string[] blockList = new[] { "HOH", "H2O", "WATER" };

            using (var reader = new PdbReader(pdbFileName))
            using (var backupWriter = new PdbWriter(backupPdbFileName))
            {
                var records = new List<Record>();
                var chainWriters = new Dictionary<(string, string, int), PdbWriter?>();

                while (true)
                {
                    var record = reader.ReadRecord();
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

                            var key = (chainId, resName, resSeq);

                            if (!chainWriters.ContainsKey(key))
                            {
                                string resFileName = $"{(entryPrefix ? $"{Entry}_" : "")}{chainId}_{resName}{(resSeq == 0 ? "" : $"_{resSeq}")}.pdb";
                                string resFilePath = Path.Combine(outputDir, resFileName);

                                var exists = File.Exists(resFilePath);

                                if (!overwrite && exists)
                                {
                                    chainWriters[key] = null;
                                    Logger.Warning($"[RCSB] Skipped existing fragment '{resFileName}' for '{Entry}'");
                                }
                                else
                                {
                                    var chainWriter = new PdbWriter(resFilePath);
                                    chainWriters[key] = chainWriter;

                                    foreach (var rec in records)
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
                        foreach (var chainWriter in chainWriters.Values)
                        {
                            chainWriter?.WriteRecord(record);
                        }
                    }

                    backupWriter.WriteRecord(record);
                }

                backupWriter.Close();
                foreach (var chainWriter in chainWriters.Values)
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
}

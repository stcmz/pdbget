using pdbget.Helpers;
using pdbget.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace pdbget
{
    internal class DownloadAction : IAction<DownloadOptions>, IDisposable
    {
        public DownloadParameters? Parameters { get; private set; }

        public int Setup(DownloadOptions options)
        {
            // Default output directory to work directory if not set
            if (options.Out == null)
                options.Out = new DirectoryInfo(Environment.CurrentDirectory);

            if (!options.Out.Exists)
                options.Out.Create();

            Stream stream;
            if (options.List == null)
            {
                stream = Console.OpenStandardInput();
                Logger.Info("Enter PDB or UniProt entries to download, press Ctrl+Z and then Enter to exit");
            }
            else
            {
                stream = options.List.OpenRead();
            }

            ThreadPool.GetMaxThreads(out int _, out int ioThreads);

            if (!ThreadPool.SetMaxThreads(options.Threads, ioThreads))
            {
                Logger.Warning($"Unable to set maximum worker threads to {options.Threads}");
            }

            string? tmpDir = Path.GetTempFileName();
            File.Delete(tmpDir);
            Directory.CreateDirectory(tmpDir);

            Parameters = new DownloadParameters(
                stream,
                options.Out,
                options.Split,
                options.Flatten,
                options.Original,
                options.Overwrite,
                "original",
                tmpDir,
                options.Threads,
                options.Timeout
            );

            return 0;
        }

        /// <summary>
        /// The input stream contains a space or line-feed separated list of PDB/UniProt entries.
        /// Target layout is determined by the --split, --flatten and --original option, as well as the per line labels.
        /// The leading label on any input line is used as the name of the grouping folder.
        /// 
        /// [example layout without labels]
        /// for this input:
        ///   5R7Y Q9Y5Y4 5R80
        /// 
        /// if no --split or --flatten:
        ///   ./5R7Y.pdb
        ///   ./Q9Y5Y4/6D26.pdb
        ///   ./Q9Y5Y4/6D27.pdb
        ///   ./5R80.pdb
        /// 
        /// if no --split but --flatten:
        ///   ./5R7Y.pdb
        ///   ./6D26.pdb
        ///   ./6D27.pdb
        ///   ./5R80.pdb
        /// 
        /// if --split but no --flatten:
        ///   ./5R7Y/A_AminoAcids.pdb
        ///   ./5R7Y/A_CL_1006.pdb
        ///   ./5R7Y/...
        ///   ./5R7Y/5R7Y.pdb => --original=inplace
        ///   ./Q9Y5Y4/6D26/A_AminoAcids.pdb
        ///   ./Q9Y5Y4/6D26/A_YCM_2308.pdb
        ///   ./Q9Y5Y4/6D26/...
        ///   ./Q9Y5Y4/6D26/6D26.pdb => --original=inplace
        ///   ./Q9Y5Y4/6D27/A_AminoAcids.pdb
        ///   ./Q9Y5Y4/6D27/A_YCM_2308.pdb
        ///   ./Q9Y5Y4/6D27/...
        ///   ./Q9Y5Y4/6D27/6D27.pdb => --original=inplace
        ///   ./5R80/A_AminoAcids.pdb
        ///   ./5R80/A_DMS_401.pdb
        ///   ./5R80/...
        ///   ./5R80/5R80.pdb => --original=inplace
        ///   ./original/5R7Y.pdb => --original=separate
        ///   ./original/Q9Y5Y4/6D26.pdb => --original=separate
        ///   ./original/Q9Y5Y4/6D27.pdb => --original=separate
        ///   ./original/5R80.pdb => --original=separate
        /// 
        /// if --split and --flatten:
        ///   ./5R7Y/A_AminoAcids.pdb
        ///   ./5R7Y/A_CL_1006.pdb
        ///   ./5R7Y/...
        ///   ./5R7Y/5R7Y.pdb => --original=inplace
        ///   ./6D26/A_AminoAcids.pdb
        ///   ./6D26/A_YCM_2308.pdb
        ///   ./6D26/...
        ///   ./6D26/6D26.pdb => --original=inplace
        ///   ./6D27/A_AminoAcids.pdb
        ///   ./6D27/A_YCM_2308.pdb
        ///   ./6D27/...
        ///   ./6D27/6D27.pdb => --original=inplace
        ///   ./5R80/A_AminoAcids.pdb
        ///   ./5R80/A_DMS_401.pdb
        ///   ./5R80/...
        ///   ./5R80/5R80.pdb => --original=inplace
        ///   ./original/5R7Y.pdb => --original=separate
        ///   ./original/6D26.pdb => --original=separate
        ///   ./original/6D27.pdb => --original=separate
        ///   ./original/5R80.pdb => --original=separate
        /// 
        /// [example layout with labels]
        /// for this input:
        ///   3CL pro:5R7Y 5R80
        ///   GPCR:Q9Y5Y4
        /// 
        /// which is equivalent to:
        ///   3CL pro: 5R7Y => leading/trailing spaces are ignored
        ///   GPCR   : Q9Y5Y4 => only one label and one colon per line is allowed
        ///   3CL pro: 5R80 => same label means same folder
        /// 
        /// if no --split or --flatten
        ///   ./3CL pro/5R7Y.pdb
        ///   ./GPCR/Q9Y5Y4/6D26.pdb
        ///   ./GPCR/Q9Y5Y4/6D27.pdb
        ///   ./3CL pro/5R80.pdb
        /// 
        /// if no --split but --flatten
        ///   ./3CL pro/5R7Y.pdb
        ///   ./GPCR/6D26.pdb
        ///   ./GPCR/6D27.pdb
        ///   ./3CL pro/5R80.pdb
        /// 
        /// if --split but no --flatten
        ///   ./3CL pro/5R7Y/A_AminoAcids.pdb
        ///   ./3CL pro/5R7Y/A_CL_1006.pdb
        ///   ./3CL pro/5R7Y/...
        ///   ./3CL pro/5R7Y/5R7Y.pdb => --original=inplace
        ///   ./GPCR/Q9Y5Y4/6D26/A_AminoAcids.pdb
        ///   ./GPCR/Q9Y5Y4/6D26/A_YCM_2308.pdb
        ///   ./GPCR/Q9Y5Y4/6D26/...
        ///   ./GPCR/Q9Y5Y4/6D26/6D26.pdb => --original=inplace
        ///   ./GPCR/Q9Y5Y4/6D27/A_AminoAcids.pdb
        ///   ./GPCR/Q9Y5Y4/6D27/A_YCM_2308.pdb
        ///   ./GPCR/Q9Y5Y4/6D27/...
        ///   ./GPCR/Q9Y5Y4/6D27/6D27.pdb => --original=inplace
        ///   ./3CL pro/5R80/A_AminoAcids.pdb
        ///   ./3CL pro/5R80/A_DMS_401.pdb
        ///   ./3CL pro/5R80/...
        ///   ./3CL pro/5R80/5R80.pdb => --original=inplace
        ///   ./original/3CL pro/5R7Y.pdb => --original=separate
        ///   ./original/GPCR/Q9Y5Y4/6D26.pdb => --original=separate
        ///   ./original/GPCR/Q9Y5Y4/6D27.pdb => --original=separate
        ///   ./original/3CL pro/5R80.pdb => --original=separate
        ///   ./original/5R7Y.pdb => --original=nolabel
        ///   ./original/Q9Y5Y4/6D26.pdb => --original=nolabel
        ///   ./original/Q9Y5Y4/6D27.pdb => --original=nolabel
        ///   ./original/5R80.pdb => --original=nolabel
        /// 
        /// if --split and --flatten
        ///   ./3CL pro/5R7Y/A_AminoAcids.pdb
        ///   ./3CL pro/5R7Y/A_CL_1006.pdb
        ///   ./3CL pro/5R7Y/...
        ///   ./3CL pro/5R7Y/5R7Y.pdb => --original=inplace
        ///   ./GPCR/6D26/A_AminoAcids.pdb
        ///   ./GPCR/6D26/A_YCM_2308.pdb
        ///   ./GPCR/6D26/...
        ///   ./GPCR/6D26/6D26.pdb => --original=inplace
        ///   ./GPCR/6D27/A_AminoAcids.pdb
        ///   ./GPCR/6D27/A_YCM_2308.pdb
        ///   ./GPCR/6D27/...
        ///   ./GPCR/6D27/6D27.pdb => --original=inplace
        ///   ./3CL pro/5R80/A_AminoAcids.pdb
        ///   ./3CL pro/5R80/A_DMS_401.pdb
        ///   ./3CL pro/5R80/...
        ///   ./3CL pro/5R80/5R80.pdb => --original=inplace
        ///   ./original/3CL pro/5R7Y.pdb => --original=separate
        ///   ./original/GPCR/6D26.pdb => --original=separate
        ///   ./original/GPCR/6D27.pdb => --original=separate
        ///   ./original/3CL pro/5R80.pdb => --original=separate
        ///   ./original/5R7Y.pdb => --original=nolabel
        ///   ./original/6D26.pdb => --original=nolabel
        ///   ./original/6D27.pdb => --original=nolabel
        ///   ./original/5R80.pdb => --original=nolabel
        /// </summary>
        public int Run()
        {
            Debug.Assert(Parameters != null);

            using var reader = new StreamReader(Parameters.Stream);
            using var countdown = new CountdownEvent(1);

            var downloadJobs = new ConcurrentDictionary<string, WorkItem>();
            var copyJobs = new ConcurrentDictionary<string, int>();
            var splitJobs = new ConcurrentDictionary<string, int>();

            int lineNum = 0;
            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine()?.Trim();
                if (line == null)
                    break;

                lineNum++;

                // Skip comments and blank lines
                if (line.StartsWith("#") || line.Length == 0)
                    continue;

                int idxFirst = line.IndexOf(':'), idxLast = line.LastIndexOf(':');
                string? label = null;

                // Multiple labels
                if (idxFirst != idxLast)
                {
                    Logger.Error($"Only one label is allowed (line {lineNum})");
                    continue;
                }

                // Extract and validate the label
                if (idxFirst >= 0)
                {
                    label = line[..idxFirst].TrimEnd();

                    char[] invalidChars = Path.GetInvalidPathChars();
                    int invalidCount = label.Count(c => invalidChars.Contains(c));

                    if (invalidCount > 0)
                    {
                        Logger.Error($"Label '{label}' contains {invalidCount} special character{(invalidCount > 1 ? "s" : "")} (line {lineNum})");
                        continue;
                    }

                    line = line[(idxFirst + 1)..].TrimStart();
                }

                // Parse the entries from the line, any ASCII white spaces is a separator
                string[]? entries = line
                    .Split("\t\n\v\f\r\x20\xA0\x85".ToCharArray(),
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct()
                    .ToArray();

                foreach (string? entry in entries)
                {
                    // Check if PDB entry
                    if (entry.Length == 4)
                    {
                        if (!entry.All(c => char.IsLetterOrDigit(c)))
                        {
                            Logger.Error($"Invalid PDB entry '{entry}' (line {lineNum})");
                            continue;
                        }

                        var config = new JobConfig(entry, null, label);
                        TryQueueJob(config, lineNum, countdown, downloadJobs, copyJobs, splitJobs);
                    }
                    // Check if UniProt entry
                    else if (4 < entry.Length && entry.Length < 15)
                    {
                        if (!entry.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
                        {
                            Logger.Error($"Invalid UniProt entry '{entry}' (line {lineNum})");
                            continue;
                        }

                        var structs = new UniProt(entry).GetStructures();

                        if (structs == null)
                        {
                            Logger.Error($"Unable to resolve UniProt entry '{entry}' (line {lineNum})");
                            continue;
                        }

                        foreach (var structure in structs)
                        {
                            var config = new JobConfig(structure.PdbEntry, entry, label);
                            TryQueueJob(config, lineNum, countdown, downloadJobs, copyJobs, splitJobs);
                        }
                    }
                    else
                    {
                        Logger.Error($"Unrecognized entry '{entry}' (line {lineNum})");
                        continue;
                    }
                }
            }

            countdown.Signal();
            countdown.Wait();

            foreach (var (pdb, control) in downloadJobs)
                control.DownloadComplete.Dispose();

            return 0;
        }

        private bool TryQueueJob(
            JobConfig config,
            int lineNum,
            CountdownEvent countdown,
            ConcurrentDictionary<string, WorkItem> downloadJobs,
            ConcurrentDictionary<string, int> copyTargets,
            ConcurrentDictionary<string, int> splitTargets)
        {
            Debug.Assert(Parameters != null);

            var downloadCompleteEvent = new ManualResetEventSlim();
            var mainJob = new WorkItem(config, lineNum, countdown, downloadCompleteEvent, new Result());

            // Test if the PDB entry has been seen in thread safe way
            // Failure to add implies the PDB entry has been seen
            if (downloadJobs.TryAdd(config.Pdb, mainJob))
            {
#if DEBUG
                Logger.Debug($"Main job {config.Label}/{config.UniProt}/{config.Pdb} (line {lineNum})");
#endif

                countdown.AddCount();
                ThreadPool.QueueUserWorkItem(DownloadPdb, mainJob, false);

                if (Parameters.Split)
                {
#if DEBUG
                    Logger.Debug($"Main-split job {config.Label}/{config.UniProt}/{config.Pdb} (line {lineNum})");
#endif

                    // First seen PDB always queues
                    countdown.AddCount();
                    var workItem = new WorkItem(config, lineNum, countdown, downloadCompleteEvent, new Result(), mainJob);
                    ThreadPool.QueueUserWorkItem(SplitPdb, workItem, false);
                }

                return true;
            }

            // Download-job's completion event is no longer needed
            downloadCompleteEvent.Dispose();

            var parentJob = downloadJobs[config.Pdb];

            var targetFragmentDir = GetOutputPath(config, false, false);
            var targetOriginalFile = GetOutputPath(config, true, false, true);

            var parentFragmentDir = GetOutputPath(parentJob.Config, false, false);
            var parentOriginalFile = GetOutputPath(parentJob.Config, true, false, true);

            // Parent is not in the copy-targets so any parent-equivalent (same target path) job need to be excluded
            // explicitly to avoid conflict
            // Identical job config results in same original file and same fragment dir as parent
            // Identical fragment dir also results in same original file
            // Early exit for identical original file applies to non-split only
            // For identical original file as parent but with split, the fragment may be different and shall not exit here
            if (parentJob.Config == config
                || parentFragmentDir == targetFragmentDir
                || !Parameters.Split && parentOriginalFile == targetOriginalFile)
            {
#if DEBUG
                Logger.Debug($"No parent-eq-job {targetFragmentDir}, {targetOriginalFile} (line {lineNum})");
#endif
                return false;
            }

            bool queued = false;

            // Same original file as parent or any copy target, no need to copy from parent output
            if (parentOriginalFile == targetOriginalFile
                || !copyTargets.TryAdd(targetOriginalFile, 0))
            {
#if DEBUG
                Logger.Debug($"No copy-job {targetOriginalFile} (line {lineNum})");
#endif
            }
            else
            {
#if DEBUG
                Logger.Debug($"Copy job {targetOriginalFile} (line {lineNum})");
#endif

                countdown.AddCount();
                var workItem = new WorkItem(config, lineNum, countdown, parentJob.DownloadComplete, new Result(), parentJob);
                ThreadPool.QueueUserWorkItem(CopyPdb, workItem, false);

                queued = true;
            }

            if (Parameters.Split)
            {
                // Same fragment dir as any split target, no need to split from parent output
                if (!splitTargets.TryAdd(targetFragmentDir, 0))
                {
#if DEBUG
                    Logger.Debug($"No split-job {targetFragmentDir} (line {lineNum})");
#endif
                }
                else
                {
#if DEBUG
                    Logger.Debug($"Split job {targetFragmentDir} (line {lineNum})");
#endif

                    countdown.AddCount();
                    var workItem = new WorkItem(config, lineNum, countdown, parentJob.DownloadComplete, new Result(), parentJob);
                    ThreadPool.QueueUserWorkItem(SplitPdb, workItem, false);

                    queued = true;
                }
            }

            return queued;
        }

        public void Dispose()
        {
            if (Parameters != null)
            {
                Parameters.Stream.Dispose();
                Directory.Delete(Parameters.TempDir, true);
            }
        }

        private record JobConfig
        (
            string Pdb,
            string? UniProt,
            string? Label
        );

        private class Result
        {
            public bool Succeeded { get; set; }
            public bool Overwritten { get; set; }
            public bool Skipped { get; set; }
        }

        private record WorkItem
        (
            JobConfig Config,
            int LineNum,
            CountdownEvent Countdown,
            ManualResetEventSlim DownloadComplete,
            Result Result,
            WorkItem? Parent = null
        );

        private string GetOutputPath(JobConfig config, bool original, bool outDir = true, bool pdbName = false)
        {
            Debug.Assert(Parameters != null);

            // Pattern: {out}/original/{label}/{uniprot}/{pdb}/{pdb}.pdb

            if (original && Parameters.Split && Parameters.Original == Original.delete)
                return pdbName ? Path.Combine(Parameters.TempDir, $"{config.Pdb}.pdb") : Parameters.TempDir;

            List<string> segments = new();

            if (outDir)
                segments.Add(Parameters.Out.FullName);

            if (original && Parameters.Split && Parameters.Original != Original.inplace)
                segments.Add(Parameters.OriginalName);

            if (config.Label != null && (!original || Parameters.Original != Original.nolabel))
                segments.Add(config.Label);

            if (config.UniProt != null && !Parameters.Flatten)
                segments.Add(config.UniProt);

            if (!original || Parameters.Split && Parameters.Original == Original.inplace)
                //if (Parameters.Split && (!original || Parameters.Original == Original.inplace))
                segments.Add(config.Pdb);

            if (pdbName)
                segments.Add($"{config.Pdb}.pdb");

            return Path.Combine(segments.ToArray());
        }

        private void DownloadPdb(WorkItem workItem)
        {
            Debug.Assert(Parameters != null);

            string pdbFileDir = GetOutputPath(workItem.Config, true);
            string pdbFileName = Path.Combine(pdbFileDir, $"{workItem.Config.Pdb}.pdb");

            bool exists = File.Exists(pdbFileName);
            var rcsb = new Rcsb(workItem.Config.Pdb);

            if (!Parameters.Overwrite && exists)
            {
                Logger.Warning($"Skipped existing '{pdbFileName}' (line {workItem.LineNum})");

                workItem.Result.Succeeded = true;
                workItem.Result.Skipped = true;
            }
            else
            {
                // Create dir if both file and dir not exist
                if (!exists)
                    Directory.CreateDirectory(pdbFileDir);

                Logger.Info($"Resolving {workItem.Config.Pdb}");

                try
                {
                    rcsb.DownloadPdb(pdbFileName);

                    if (exists)
                    {
                        Logger.Warning($"Overwrote '{pdbFileName}' (line {workItem.LineNum})");

                        workItem.Result.Succeeded = true;
                        workItem.Result.Overwritten = true;
                    }
                    else
                    {
                        Logger.Info($"Downloaded '{pdbFileName}'");

                        workItem.Result.Succeeded = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unable to download {workItem.Config.Pdb}.pdb (line {workItem.LineNum}), message: {ex.Message}");

                    workItem.Result.Succeeded = false;
                }
            }

            workItem.DownloadComplete.Set();
            workItem.Countdown.Signal();
        }

        private void CopyPdb(WorkItem workItem)
        {
            Debug.Assert(Parameters != null);
            Debug.Assert(workItem.Parent != null);

            string pdbFileDir = GetOutputPath(workItem.Config, true);
            string pdbFileName = Path.Combine(pdbFileDir, $"{workItem.Config.Pdb}.pdb");

            bool exists = File.Exists(pdbFileName);

            if (!Parameters.Overwrite && exists)
            {
                Logger.Warning($"Skipped existing '{pdbFileName}' (line {workItem.LineNum})");

                workItem.Result.Succeeded = true;
                workItem.Result.Skipped = true;
            }
            else
            {
                // Wait until parent job completes
                workItem.DownloadComplete.Wait();

                if (!workItem.Parent.Result.Succeeded)
                {
                    Logger.Warning($"Skipped '{pdbFileName}' due to previous failure (line {workItem.LineNum})");

                    workItem.Result.Succeeded = false;
                    workItem.Result.Skipped = true;
                }
                else
                {
                    try
                    {
                        // Create dir if both file and dir not exist
                        if (!exists)
                            Directory.CreateDirectory(pdbFileDir);

                        string parentFileName = GetOutputPath(workItem.Parent.Config, true, true, true);
                        File.Copy(parentFileName, pdbFileName, true);

                        if (exists)
                        {
                            Logger.Warning($"Overwrote '{pdbFileName}' (line {workItem.LineNum})");

                            workItem.Result.Succeeded = true;
                            workItem.Result.Overwritten = true;
                        }
                        else
                        {
                            Logger.Info($"Copied '{pdbFileName}'");

                            workItem.Result.Succeeded = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Unable to create '{pdbFileName}' (line {workItem.LineNum}), message: {ex.Message}");

                        workItem.Result.Succeeded = false;
                    }
                }
            }

            workItem.Countdown.Signal();
        }

        private void SplitPdb(WorkItem workItem)
        {
            Debug.Assert(Parameters != null);
            Debug.Assert(workItem.Parent != null);

            workItem.DownloadComplete.Wait();

            if (!workItem.Parent.Result.Succeeded)
            {
                Logger.Warning($"Skipped splitting {workItem.Config.Pdb}.pdb due to previous failure (line {workItem.LineNum})");

                workItem.Result.Succeeded = false;
                workItem.Result.Skipped = true;
            }
            else
            {
                string originalFile = GetOutputPath(workItem.Parent.Config, true, true, true);

                try
                {
                    string fragmentDir = GetOutputPath(workItem.Config, false);
                    Directory.CreateDirectory(fragmentDir);

                    var rcsb = new Rcsb(workItem.Config.Pdb);
                    rcsb.SplitPdbChains(originalFile, fragmentDir, false, Parameters.Overwrite);

                    workItem.Result.Succeeded = true;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unsuccessful split of '{originalFile}' (line {workItem.LineNum}), message: {ex.Message}");

                    workItem.Result.Succeeded = false;
                }
            }

            workItem.Countdown.Signal();
        }
    }
}

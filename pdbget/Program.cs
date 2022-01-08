using pdbget.Helpers;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace pdbget
{
    public static class Program
    {
        public static async Task<int> Main(params string[] args)
        {
            // See .NET command line API here: https://github.com/dotnet/command-line-api

            // Create a root command with some options
            var rootCommand = new RootCommand("A command line tool for fetching and splitting PDB files by Maozi Chen")
            {
                new Option<FileInfo>(
                    new[] { "-l", "--list" },
                    "A file containing the downloading entry list (can be a mix of PDB entries and UniProt entries, and optionally with a label per line in format 'label1: entry1 entry2'). If --list absent, the program will read from standard input (or pipe input if redirected)"),
                new Option<DirectoryInfo>(
                    new[] { "-o", "--out" },
                    "A directory to store the downloaded PDB files and possibly the split chains [default: .]"),
                new Option<bool>(
                    new[] { "-s", "--split" },
                    "To split the downloaded PDB files into chains and small molecules"),
                new Option<bool>(
                    new[] { "-f", "--flatten" },
                    "To flatten the output directory for UniProt entries, i.e. equivalent to replacing a UniProt entry with its PDB entries"),
                new Option<Original>(
                    new[] { "-O", "--original" },
                    () => Original.inplace,
                    "The strategy for placing the original PDB files when --split enabled, ignored if --split absent"),
                new Option<bool>(
                    new[] { "-y", "--overwrite" },
                    "Force to overwriting all existing output files, the default behavior is to skip existing files"),
                new Option<int>(
                    new[] { "--threads" },
                    () => Environment.ProcessorCount,
                    "The maximum number of worker threads for concurrent downloading"),
                new Option<int>(
                    new[] { "--timeout" },
                    () => 30,
                    "Maximum wait time in seconds for downloading a single PDB file"),
            };

            // Add a validator to the pipeline for validating directory options
            rootCommand.AddValidator(cr =>
            {
                var x = cr.Tokens;

                return null;
            });

            // The parameters of the handler method are matched according to the names of the options
            rootCommand.Handler = CommandHandler.Create<DownloadOptions>(options =>
            {
                using var action = new DownloadAction();

                int retCode = action.Setup(options);

                if (retCode != 0)
                    return retCode;

                var sw = Stopwatch.StartNew();
                retCode = action.Run();

                if (retCode == 0)
                    Logger.Info($"Total time used: {sw.Elapsed}");

                return retCode;
            });

            // Parse the incoming args and invoke the handler
            return await rootCommand.InvokeAsync(args);
        }
    }
}

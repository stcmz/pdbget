using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace pdbget.Helpers
{
    internal static class ShellHelper
    {
        public static (string program, string arguments) SplitCommandLine(this string commandLine)
        {
            int cmdBegin = 0, cmdEnd = commandLine.Length;
            if (commandLine[0] == '"' || commandLine[0] == '\'')
            {
                int idx = commandLine.IndexOf(commandLine[0], 1);
                if (idx != -1)
                {
                    cmdBegin = 1;
                    cmdEnd = idx;
                }
            }
            else
            {
                int idx = commandLine.IndexOf(' ');
                if (idx != -1)
                {
                    cmdBegin = 0;
                    cmdEnd = idx;
                }
            }

            return (commandLine[cmdBegin..cmdEnd], commandLine[(cmdEnd + 1)..].TrimStart());
        }

        public static void RunCommand(this string program, string arguments, out string stdout, out string stderr)
        {
            // Create your Process
            using var process = new Process();
            process.StartInfo.FileName = program;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            var sberr = new StringBuilder();

            // Set ONLY ONE handler here.
            process.ErrorDataReceived += (sender, e) =>
            {
                sberr.AppendLine(e.Data);
            };

            // Start process
            process.Start();

            // Read one element asynchronously
            process.BeginErrorReadLine();

            // Read the other one synchronously
            stdout = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
            stderr = sberr.ToString();
        }
    }
}

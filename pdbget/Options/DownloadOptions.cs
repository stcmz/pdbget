using System.IO;

namespace pdbget
{
    internal class DownloadOptions
    {
        public FileInfo? List { get; set; }
        public DirectoryInfo? Out { get; set; }
        public bool Split { get; set; }
        public bool Flatten { get; set; }
        public Original Original { get; set; }
        public bool Overwrite { get; set; }
        public int Threads { get; set; }
        public int Timeout { get; set; }
    }
}

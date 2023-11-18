using System.IO;

namespace pdbget;

internal record DownloadParameters
(
    Stream Stream,
    DirectoryInfo Out,
    bool Split,
    bool Flatten,
    Original Original,
    bool Overwrite,
    string OriginalName,
    string TempDir,
    int Threads,
    int Timeout
);

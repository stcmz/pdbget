using System;
using System.IO;

namespace pdbget.PdbFormat;

public class PdbWriter : IDisposable
{
    private readonly TextWriter _writer;

    public PdbWriter(TextWriter writer)
    {
        _writer = writer;
    }

    public PdbWriter(Stream stream)
    {
        _writer = new StreamWriter(stream)
        {
            NewLine = "\n"
        };
    }

    public PdbWriter(string path)
    {
        _writer = new StreamWriter(path)
        {
            NewLine = "\n"
        };
    }

    public void WriteRecord(Record line)
    {
        _writer.WriteLine(line.ToRow());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _writer.Dispose();
    }

    public void Close()
    {
        _writer.Close();
    }
}

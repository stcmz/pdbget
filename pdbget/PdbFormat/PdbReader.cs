using System;
using System.IO;

namespace pdbget.PdbFormat
{
    public class PdbReader : IDisposable
    {
        private readonly TextReader _reader;
        private readonly Stream? _stream;

        public PdbReader(TextReader reader)
        {
            _reader = reader;
        }

        public PdbReader(Stream stream)
        {
            _reader = new StreamReader(stream);
        }

        public PdbReader(string path)
        {
            _stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            _reader = new StreamReader(_stream);
        }

        public Record? ReadRecord()
        {
            string? line = _reader.ReadLine();
            if (line == "END")
                return EndRecord.CreateFromRow(line);

            if (line == null || line.Length > 80)
                return null;

            string tag = line.Substring(0, 6);
            if (!tag.TryParseRecordType(out var recordType))
            {
                Console.WriteLine($"Failed: unknown record type {tag}");
                return null;
            }

            try
            {
                return recordType switch
                {
                    // Primary Structure Section
                    RecordType.DbRef => DbRefRecord.CreateFromRow(line),
                    RecordType.DbRef1 => DbRef1Record.CreateFromRow(line),
                    RecordType.DbRef2 => DbRef2Record.CreateFromRow(line),
                    RecordType.SeqAdv => SeqAdvRecord.CreateFromRow(line),
                    RecordType.SeqRes => SeqResRecord.CreateFromRow(line),
                    RecordType.ModRes => ModResRecord.CreateFromRow(line),

                    // Coordinate Section
                    RecordType.Model => ModelRecord.CreateFromRow(line),
                    RecordType.Atom => AtomRecord.CreateFromRow(line),
                    RecordType.Anisou => AnisouRecord.CreateFromRow(line),
                    RecordType.Terminated => TerminatedRecord.CreateFromRow(line),
                    RecordType.HeteroAtom => HeteroAtomRecord.CreateFromRow(line),
                    RecordType.EndModel => EndModelRecord.CreateFromRow(line),

                    // Others
                    _ => UnknownRecord.CreateFromRow(line),
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Close()
        {
            _reader.Close();
            _stream?.Close();
        }

        public void Dispose()
        {
            _reader.Dispose();
            _stream?.Dispose();
        }
    }
}

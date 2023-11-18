namespace pdbget.PdbFormat;

public abstract class Record
{
    public abstract string Tag { get; }
    public abstract RecordType Type { get; }
    public abstract void FromRow(string row);
    public abstract string ToRow();
}

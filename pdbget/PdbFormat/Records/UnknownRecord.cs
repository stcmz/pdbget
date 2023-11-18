namespace pdbget.PdbFormat;

public class UnknownRecord : Record
{
    public override string Tag => _tag ?? string.Empty;
    public override RecordType Type => _type;

    private string? _tag;
    private RecordType _type;

    public string? RawRow { get; set; }

    public override void FromRow(string row)
    {
        RawRow = row;
        _tag = row[..6].Trim();
        _type = _tag.ParseRecordType();
    }

    public override string ToRow()
    {
        return RawRow ?? string.Empty;
    }

    public static UnknownRecord CreateFromRow(string row)
    {
        UnknownRecord record = new();
        record.FromRow(row);
        return record;
    }
}

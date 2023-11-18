namespace pdbget.PdbFormat;

/// <summary>
/// The MODEL record specifies the model serial number when multiple models of the same structure are presented in a single
/// coordinate entry, as is often the case with structures determined by NMR. See
/// https://www.wwpdb.org/documentation/file-format-content/format33/sect9.html#MODEL
/// 
/// COLUMNS        DATA  TYPE    FIELD          DEFINITION
/// ---------------------------------------------------------------------------------------
///  1 -  6        Record name   "MODEL "
/// 11 - 14        Integer       serial         Model serial number.
/// </summary>
public class ModelRecord : Record
{
    public override string Tag => "MODEL";
    public override RecordType Type => RecordType.Model;

    public int Serial { get; set; }

    public override void FromRow(string row)
    {
        Serial = int.Parse(row.Substring(10, 4)); // 11 - 14
    }

    public override string ToRow()
    {
        return $"{Tag,-6}    {Serial,4}{"",66}";      //  1 - 80
    }

    public static ModelRecord? CreateFromRow(string row)
    {
        ModelRecord record = new();

        if (row[..6].Trim() != record.Tag)
            return null;

        record.FromRow(row);
        return record;
    }
}

namespace pdbget.PdbFormat;

/// <summary>
/// The ENDMDL records are paired with MODEL records to group individual structures found in a coordinate entry. See
/// https://www.wwpdb.org/documentation/file-format-content/format33/sect9.html#ENDMDL
/// 
/// COLUMNS       DATA  TYPE     FIELD        DEFINITION
/// ------------------------------------------------------------------
/// 1 - 6         Record name   "ENDMDL"
/// </summary>
/// <returns></returns>
public class EndModelRecord : Record
{
    public override string Tag => "ENDMDL";
    public override RecordType Type => RecordType.EndModel;

    public override void FromRow(string row)
    {
    }

    public override string ToRow()
    {
        return $"{Tag,-80}";    //  1 -  80
    }

    public static EndModelRecord? CreateFromRow(string row)
    {
        EndModelRecord record = new();

        if (row[..6].Trim() != record.Tag)
            return null;

        record.FromRow(row);
        return record;
    }
}

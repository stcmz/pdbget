namespace pdbget.PdbFormat
{
    /// <summary>
    /// The END record marks the end of the PDB file.
    /// https://www.wwpdb.org/documentation/file-format-content/format33/sect11.html#END
    /// 
    /// COLUMNS       DATA  TYPE     FIELD         DEFINITION
    /// -------------------------------------------------------
    /// 1 -  6        Record name    "END   "
    /// </summary>
    public class EndRecord : Record
    {
        public override string Tag => "END";
        public override RecordType Type => RecordType.End;

        public override void FromRow(string row)
        {
        }

        public override string ToRow()
        {
            return Tag;
        }

        public static EndRecord? CreateFromRow(string row)
        {
            var record = new EndRecord();

            if (row != record.Tag)
                return null;

            record.FromRow(row);
            return record;
        }
    }
}

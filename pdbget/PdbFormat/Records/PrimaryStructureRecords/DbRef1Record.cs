namespace pdbget.PdbFormat
{
    /// <summary>
    /// This updated two-line format is used when the accession code or sequence numbering does not fit the space allotted in the
    /// standard DBREF format. This includes some GenBank sequence numbering (greater than 5 characters) and UNIMES accession numbers
    /// (greater than 12 characters).
    /// https://www.wwpdb.org/documentation/file-format-content/format33/sect3.html#DBREF1
    /// 
    /// COLUMNS        DATA  TYPE    FIELD         DEFINITION
    /// -----------------------------------------------------------------------------------
    ///  1 -  6        Record name   "DBREF1"
    ///  8 - 11        IDcode        idCode        ID code of this entry.
    /// 13             Character     chainID       Chain identifier.
    /// 15 - 18        Integer       seqBegin      Initial sequence number of the PDB sequence segment, right justified.
    /// 19             AChar         insertBegin   Initial insertion code of the PDB sequence segment.
    /// 21 - 24        Integer       seqEnd        Ending sequence number of the PDB sequence segment, right justified.
    /// 25             AChar         insertEnd     Ending insertion code of the PDB sequence  segment.
    /// 27 - 32        LString       database      Sequence database name.
    /// 48 - 67        LString       dbIdCode      Sequence database identification code, left justified.
    /// </summary>
    public class DbRef1Record : AbstractChainedRecord
    {
        public override string Tag => "DBREF1";
        public override RecordType Type => RecordType.DbRef1;

        public string? IdCode { get; set; }
        public int SeqBegin { get; set; }
        public char InsertBegin { get; set; }
        public int SeqEnd { get; set; }
        public char InsertEnd { get; set; }
        public string? Database { get; set; }
        public string? DbIdCode { get; set; }

        public override void FromRow(string row)
        {
            IdCode = row.Substring(7, 4).Trim();        //  8 - 11
            ChainId = row[12];                          // 13
            SeqBegin = int.Parse(row.Substring(14, 4)); // 15 - 18
            InsertBegin = row[18];                      // 19
            SeqEnd = int.Parse(row.Substring(20, 4));   // 21 - 24
            InsertEnd = row[24];                        // 25
            Database = row.Substring(26, 6).Trim();     // 27 - 32
            DbIdCode = row.Substring(47, 20).Trim();    // 48 - 67
        }

        public override string ToRow()
        {
            return $"{Tag,-6} {IdCode,4} {ChainId,1} {SeqBegin,4}{InsertBegin,1} {SeqEnd,4}{InsertEnd,1} {Database,-6}{"",15}{DbIdCode,-20}{"",13}";
        }

        public static DbRef1Record? CreateFromRow(string row)
        {
            var record = new DbRef1Record();

            if (row.Substring(0, 6).Trim() != record.Tag)
                return null;

            record.FromRow(row);
            return record;
        }
    }
}

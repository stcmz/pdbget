namespace pdbget.PdbFormat
{
    /// <summary>
    /// This updated two-line format is used when the accession code or sequence numbering does not fit the space allotted in the
    /// standard DBREF format. This includes some GenBank sequence numbering (greater than 5 characters) and UNIMES accession numbers
    /// (greater than 12 characters).
    /// https://www.wwpdb.org/documentation/file-format-content/format33/sect3.html#DBREF1
    /// 
    /// DBREF2
    /// COLUMNS       DATA  TYPE    FIELD         DEFINITION
    /// -----------------------------------------------------------------------------------
    ///  1 -  6       Record name   "DBREF2"
    ///  8 - 11       IDcode        idCode        ID code of this entry.
    /// 13            Character     chainID       Chain identifier.
    /// 19 - 40       LString       dbAccession   Sequence database accession code, left justified.
    /// 46 - 55       Integer       seqBegin      Initial sequence number of the Database segment, right justified.
    /// 58 - 67       Integer       seqEnd        Ending sequence number of the Database segment, right justified.
    /// </summary>
    public class DbRef2Record : AbstractChainedRecord
    {
        public override string Tag => "DBREF2";
        public override RecordType Type => RecordType.DbRef2;

        public string? IdCode { get; set; }
        public string? DbAccession { get; set; }
        public int SeqBegin { get; set; }
        public int SeqEnd { get; set; }

        public override void FromRow(string row)
        {
            IdCode = row.Substring(7, 4).Trim();         //  8 - 11
            ChainId = row[12];                           // 13
            DbAccession = row.Substring(18, 22).Trim();  // 19 - 40
            SeqBegin = int.Parse(row.Substring(45, 10)); // 46 - 55
            SeqEnd = int.Parse(row.Substring(57, 10));   // 58 - 67
        }

        public override string ToRow()
        {
            return $"{Tag,-6} {IdCode,4} {ChainId,1}{"",5}{DbAccession,-22}{"",5}{SeqBegin,10}  {SeqEnd,10}{"",13}";
        }

        public static DbRef2Record? CreateFromRow(string row)
        {
            var record = new DbRef2Record();

            if (row.Substring(0, 6).Trim() != record.Tag)
                return null;

            record.FromRow(row);
            return record;
        }
    }
}

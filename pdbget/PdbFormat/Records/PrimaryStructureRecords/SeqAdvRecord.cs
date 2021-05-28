namespace pdbget.PdbFormat
{
    /// <summary>
    /// The SEQADV record identifies differences between sequence information in the SEQRES records of the PDB entry and the sequence
    /// database entry given in DBREF. Please note that these records were designed to identify differences and not errors.  No
    /// assumption is made as to which database contains the correct data.  A comment explaining any engineered differences in the
    /// sequence between the PDB and the sequence database may also be included here.
    /// https://www.wwpdb.org/documentation/file-format-content/format33/sect3.html#SEQADV
    /// 
    /// COLUMNS        DATA TYPE     FIELD         DEFINITION
    /// -----------------------------------------------------------------
    ///  1 -  6        Record name   "SEQADV"
    ///  8 - 11        IDcode        idCode        ID  code of this entry.
    /// 13 - 15        Residue name  resName       Name of the PDB residue in conflict.
    /// 17             Character     chainID       PDB  chain identifier.
    /// 19 - 22        Integer       seqNum        PDB  sequence number.
    /// 23             AChar         iCode         PDB insertion code.
    /// 25 - 28        LString       database
    /// 30 - 38        LString       dbAccession   Sequence  database accession number.
    /// 40 - 42        Residue name  dbRes         Sequence database residue name.
    /// 44 - 48        Integer       dbSeq         Sequence database sequence number.
    /// 50 - 70        LString       conflict      Conflict comment.
    /// </summary>
    public class SeqAdvRecord : AbstractChainedRecord
    {
        public override string Tag => "SEQADV";
        public override RecordType Type => RecordType.SeqAdv;

        public string? IdCode { get; set; }
        public string? ResidueName { get; set; }
        public int? SeqNum { get; set; }
        public char ICode { get; set; }
        public string? Database { get; set; }
        public string? DbAccession { get; set; }
        public string? DbRes { get; set; }
        public int? DbSeq { get; set; }
        public bool SeeRemark999 { get; set; }
        public string? Conflict { get; set; }

        public override void FromRow(string row)
        {
            IdCode = row.Substring(7, 4).Trim();                     //  8 - 11
            ResidueName = row.Substring(12, 3).Trim();               // 13 - 15
            ChainId = row[16];                                       // 17
            if (int.TryParse(row.Substring(18, 4), out int seqNum))  // 19 - 22
                SeqNum = seqNum;
            ICode = row[22];                                         // 23
            Database = row.Substring(24, 4).Trim();                  // 25 - 28
            DbAccession = row.Substring(29, 9).Trim();               // 30 - 38
            DbRes = row.Substring(39, 3).Trim();                     // 40 - 42
            if (int.TryParse(row.Substring(43, 5), out int dbSeq))
                DbSeq = dbSeq;                                       // 44 - 48
            SeeRemark999 = row.Substring(49).Trim() == "CONFLICT SEE REMARK 999";
            if (!SeeRemark999)
                Conflict = row.Substring(49, 21).Trim();             // 50 - 70
        }

        public override string ToRow()
        {
            return $"{Tag,-6} {IdCode,4} {ResidueName,3} {ChainId,1} {SeqNum,4}{ICode,1} {Database,-4} {DbAccession,-9} {DbRes,3} {DbSeq,5} {(SeeRemark999 ? "CONFLICT SEE REMARK 999" : Conflict),-31}";
        }

        public static SeqAdvRecord? CreateFromRow(string row)
        {
            var record = new SeqAdvRecord();

            if (row.Substring(0, 6).Trim() != record.Tag)
                return null;

            record.FromRow(row);
            return record;
        }
    }
}

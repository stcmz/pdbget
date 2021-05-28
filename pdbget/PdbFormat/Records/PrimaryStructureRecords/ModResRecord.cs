namespace pdbget.PdbFormat
{
    /// <summary>
    /// The MODRES record provides descriptions of modifications (e.g., chemical or post-translational) to protein and nucleic acid
    /// residues. Included are correlations between residue names given in a PDB entry and standard residues.
    /// https://www.wwpdb.org/documentation/file-format-content/format33/sect3.html#MODRES
    /// 
    /// COLUMNS        DATA TYPE     FIELD       DEFINITION
    /// --------------------------------------------------------------------------------
    ///  1 -  6        Record name   "MODRES"
    ///  8 - 11        IDcode        idCode      ID code of this entry.
    /// 13 - 15        Residue name  resName     Residue name used in this entry.
    /// 17             Character     chainID     Chain identifier.
    /// 19 - 22        Integer       seqNum      Sequence number.
    /// 23             AChar         iCode       Insertion code.
    /// 25 - 27        Residue name  stdRes      Standard residue name.
    /// 30 - 70        String        comment     Description of the residue modification.
    /// </summary>
    public class ModResRecord : AbstractChainedRecord
    {
        public override string Tag => "MODRES";
        public override RecordType Type => RecordType.ModRes;

        public string? IdCode { get; set; }
        public string? ResName { get; set; }
        public int? SeqNum { get; set; }
        public char ICode { get; set; }
        public string? StdRes { get; set; }
        public string? Comment { get; set; }

        public override void FromRow(string row)
        {
            IdCode = row.Substring(7, 4).Trim();       //  8 - 11
            ResName = row.Substring(12, 3).Trim();     // 13 - 15
            ChainId = row[16];                         // 17
            if (int.TryParse(row.Substring(18, 4), out int seqNum))
                SeqNum = seqNum;                       // 19 - 22
            ICode = row[22];                           // 23
            StdRes = row.Substring(24, 3).Trim();      // 25 - 27
            Comment = row.Substring(29, 41).Trim();    // 30 - 70
        }

        public override string ToRow()
        {
            return $"{Tag,-6} {IdCode,4} {ResName,3}"  //  1 - 15
                + $" {ChainId,1} {SeqNum,4}{ICode,1} " // 16 - 24
                + $"{StdRes,3}  {Comment,-41}{"",10}";  // 25 - 80
        }

        public static ModResRecord? CreateFromRow(string row)
        {
            var record = new ModResRecord();

            if (row.Substring(0, 6).Trim() != record.Tag)
                return null;

            record.FromRow(row);
            return record;
        }
    }
}

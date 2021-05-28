namespace pdbget.PdbFormat
{
    /// <summary>
    /// The TER record indicates the end of a list of ATOM/HETATM records for a chain. See
    /// https://www.wwpdb.org/documentation/file-format-content/format33/sect9.html#TER
    /// 
    /// COLUMNS        DATA  TYPE    FIELD           DEFINITION
    /// -------------------------------------------------------------------------
    ///  1 -  6        Record name   "TER   "
    ///  7 - 11        Integer       serial          Serial number.
    /// 18 - 20        Residue name  resName         Residue name.
    /// 22             Character     chainID         Chain identifier.
    /// 23 - 26        Integer       resSeq          Residue sequence number.
    /// 27             AChar         iCode           Insertion code.
    /// </summary>
    public class TerminatedRecord : AbstractChainedCoordinateRecord
    {
        public override string Tag => "TER";
        public override RecordType Type => RecordType.Terminated;

        public override void FromRow(string row)
        {
            Serial = int.Parse(row.Substring(6, 5));  //  7 - 11
            ResidueName = row.Substring(17, 3).Trim();    // 18 - 20
            ChainId = row[21];                        // 22
            ResidueSequence = int.Parse(row.Substring(22, 4)); // 23 - 26
            InsertionCode = row[26];                          // 27
        }

        public override string ToRow()
        {
            return $"{Tag,-6}{Serial,5}      {ResidueName,3}"   // 1 - 20
                + $" {ChainId}{ResidueSequence,4}{InsertionCode}"            // 21 - 27
                + $"{"",53}";                               // 28 - 80
        }

        public static TerminatedRecord? CreateFromRow(string row)
        {
            var record = new TerminatedRecord();

            if (row.Substring(0, 6).Trim() != record.Tag)
                return null;

            record.FromRow(row);
            return record;
        }
    }
}

using System;

namespace pdbget.PdbFormat
{
    public abstract class AbstractAtomRecord : AbstractChainedCoordinateRecord
    {
        public string? Name { get; set; }
        public char AltLoc { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Occupancy { get; set; }
        public float TempFactor { get; set; }
        public string? Element { get; set; }
        public string? Charge { get; set; }

        public override void FromRow(string row)
        {
            Serial = int.Parse(row.Substring(6, 5));           //  7 - 11
            Name = row.Substring(12, 4).Trim();                // 13 - 16
            AltLoc = row[16];                                  // 17
            ResidueName = row.Substring(17, 3).Trim();         // 18 - 20
            ChainId = row[21];                                 // 22
            ResidueSequence = int.Parse(row.Substring(22, 4)); // 23 - 26
            InsertionCode = row[26];                           // 27
            X = float.Parse(row.Substring(30, 8));             // 31 - 38
            Y = float.Parse(row.Substring(38, 8));             // 39 - 46
            Z = float.Parse(row.Substring(46, 8));             // 47 - 54
            Occupancy = float.Parse(row.Substring(54, 6));     // 55 - 60
            TempFactor = float.Parse(row.Substring(60, 6));    // 61 - 66
            Element = row.Substring(76, 2).Trim();             // 77 - 78
            Charge = row.Substring(78, 2).Trim();              // 79 - 80

            if (string.IsNullOrEmpty(Element))
                throw new Exception($@"ATOM line ""{row}"" doesn't have element defined");
        }

        public override string ToRow()
        {
            string recName = Name ?? string.Empty;
            if (recName.Length < 4 && Element?.Length == 1 && recName.StartsWith(Element))
                recName = ' ' + recName;
            return $"{Tag,-6}{Serial,5} {recName,-4}{AltLoc}{ResidueName,3}"  // 1 - 20
                + $" {ChainId}{ResidueSequence,4}{InsertionCode}   "          // 21 - 30
                + $"{X,8:0.000}{Y,8:0.000}{Z,8:0.000}{Occupancy,6:0.00}"      // 31 - 60
                + $"{TempFactor,6:0.00}          {Element,2}{Charge,-2}";     // 61 - 80
        }
    }
}

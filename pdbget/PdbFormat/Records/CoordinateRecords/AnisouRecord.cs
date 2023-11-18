namespace pdbget.PdbFormat;

/// <summary>
/// The ANISOU records present the anisotropic temperature factors. See
/// https://www.wwpdb.org/documentation/file-format-content/format33/sect9.html#ANISOU
/// 
/// COLUMNS       DATA  TYPE    FIELD          DEFINITION
/// -----------------------------------------------------------------
///  1 - 6        Record name   "ANISOU"
///  7 - 11       Integer       serial         Atom serial number.
/// 13 - 16       Atom          name           Atom name.
/// 17            Character     altLoc         Alternate location indicator
/// 18 - 20       Residue name  resName        Residue name.
/// 22            Character     chainID        Chain identifier.
/// 23 - 26       Integer       resSeq         Residue sequence number.
/// 27            AChar         iCode          Insertion code.
/// 29 - 35       Integer       u[0][0]        U(1,1)
/// 36 - 42       Integer       u[1][1]        U(2,2)
/// 43 - 49       Integer       u[2][2]        U(3,3)
/// 50 - 56       Integer       u[0][1]        U(1,2)
/// 57 - 63       Integer       u[0][2]        U(1,3)
/// 64 - 70       Integer       u[1][2]        U(2,3)
/// 77 - 78       LString(2)    element        Element symbol, right-justified.
/// 79 - 80       LString(2)    charge         Charge on the atom.
/// </summary>
public class AnisouRecord : AbstractChainedCoordinateRecord
{
    public override string Tag => "ANISOU";
    public override RecordType Type => RecordType.Anisou;

    public string? Name { get; set; }
    public char AltLoc { get; set; }
    public int U11 { get; set; }
    public int U22 { get; set; }
    public int U33 { get; set; }
    public int U12 { get; set; }
    public int U13 { get; set; }
    public int U23 { get; set; }
    public string? Element { get; set; }
    public string? Charge { get; set; }

    public override void FromRow(string row)
    {
        Serial = int.Parse(row.Substring(6, 5));  //  7 - 11
        Name = row.Substring(12, 4).Trim();       // 13 - 16
        AltLoc = row[16];                         // 17
        ResidueName = row.Substring(17, 3).Trim();    // 18 - 20
        ChainId = row[21];                        // 22
        ResidueSequence = int.Parse(row.Substring(22, 4)); // 23 - 26
        InsertionCode = row[26];                          // 27
        U11 = int.Parse(row.Substring(28, 7));    // 29 - 35
        U22 = int.Parse(row.Substring(35, 7));    // 36 - 42
        U33 = int.Parse(row.Substring(42, 7));    // 43 - 49
        U12 = int.Parse(row.Substring(49, 7));    // 50 - 56
        U13 = int.Parse(row.Substring(56, 7));    // 57 - 63
        U23 = int.Parse(row.Substring(63, 7));    // 64 - 70
        Element = row.Substring(76, 2).Trim();    // 77 - 78
        Charge = row.Substring(78, 2).Trim();     // 79 - 80
    }

    public override string ToRow()
    {
        string recName = Name ?? string.Empty;
        if (recName.Length < 4 && Element?.Length == 1 && recName.StartsWith(Element))
            recName = ' ' + recName;
        return $"{Tag,-6}{Serial,5} {recName,-4}{AltLoc}{ResidueName,3}" // 1 - 20
            + $" {ChainId}{ResidueSequence,4}{InsertionCode} "                        // 21 - 28
            + $"{U11,7}{U22,7}{U33,7}{U12,7}{U13,7}{U23,7}"          // 29 - 70
            + $"      {Element,2}{Charge,-2}";                       // 71 - 80
    }

    public static AnisouRecord? CreateFromRow(string row)
    {
        AnisouRecord record = new();

        if (row[..6].Trim() != record.Tag)
            return null;

        record.FromRow(row);
        return record;
    }
}

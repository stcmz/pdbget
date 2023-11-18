using System.Collections.Generic;
using System.Linq;

namespace pdbget.PdbFormat;

/// <summary>
/// SEQRES records contain a listing of the consecutive chemical components covalently linked in a linear fashion to form a
/// polymer. The chemical components included in this listing may be standard or modified amino acid and nucleic acid residues.
/// It may also include other residues that are linked to the standard backbone in the polymer. Chemical components or groups
/// covalently linked to side-chains (in peptides) or sugars and/or bases (in nucleic acid polymers) will not be listed here. 
/// https://www.wwpdb.org/documentation/file-format-content/format33/sect3.html#SEQRES
/// 
/// COLUMNS        DATA TYPE      FIELD        DEFINITION
/// -------------------------------------------------------------------------------------
///  1 -  6        Record name    "SEQRES"
///  8 - 10        Integer        serNum       Serial number of the SEQRES record for the current  chain. Starts at 1 and
///                                            increments by one  each line. Reset to 1 for each chain.
/// 12             Character      chainID      Chain identifier. This may be any single legal  character, including a blank which
///                                            is used if there is only one chain.
/// 14 - 17        Integer        numRes       Number of residues in the chain. This value is repeated on every record.
/// 20 - 22        Residue name   resName      Residue name.
/// 24 - 26        Residue name   resName      Residue name.
/// 28 - 30        Residue name   resName      Residue name.
/// 32 - 34        Residue name   resName      Residue name.
/// 36 - 38        Residue name   resName      Residue name.
/// 40 - 42        Residue name   resName      Residue name.
/// 44 - 46        Residue name   resName      Residue name.
/// 48 - 50        Residue name   resName      Residue name.
/// 52 - 54        Residue name   resName      Residue name.
/// 56 - 58        Residue name   resName      Residue name.
/// 60 - 62        Residue name   resName      Residue name.
/// 64 - 66        Residue name   resName      Residue name.
/// 68 - 70        Residue name   resName      Residue name.
/// </summary>
public class SeqResRecord : AbstractChainedRecord
{
    public override string Tag => "SEQRES";
    public override RecordType Type => RecordType.SeqRes;

    public int SerNum { get; set; }
    public int NumRes { get; set; }
    public string[] ResNames { get; set; } = [];

    public override void FromRow(string row)
    {
        SerNum = int.Parse(row.Substring(7, 3));  //  8 - 10
        ChainId = row[11];                        // 12
        NumRes = int.Parse(row.Substring(13, 4)); // 14 - 17

        List<string> resNames = [];
        for (int i = 19; i < 70; i += 4)
        {
            string resName = row.Substring(i, 3).Trim(); // i+1 - i+3
            if (resName == string.Empty)
                break;
            resNames.Add(resName);
        }

        ResNames = [.. resNames];
    }

    public override string ToRow()
    {
        return $"{Tag,-6} {SerNum,3} {ChainId,1} {NumRes,4}  " //  1 - 19
            + $"{string.Join(" ", ResNames.Select(o => $"{o,3}")),-51}" // 20 - 70
            + $"{"",10}"; // 71 - 80
    }

    public static SeqResRecord? CreateFromRow(string row)
    {
        SeqResRecord record = new();

        if (row[..6].Trim() != record.Tag)
            return null;

        record.FromRow(row);
        return record;
    }
}

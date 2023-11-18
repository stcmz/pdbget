namespace pdbget.PdbFormat;

/// <summary>
/// The DBREF record provides cross-reference links between PDB sequences (what appears in SEQRES record) and a corresponding database sequence.
/// https://www.wwpdb.org/documentation/file-format-content/format33/sect3.html#DBREF
/// 
/// COLUMNS       DATA TYPE     FIELD              DEFINITION
/// -----------------------------------------------------------------------------------
///  1 -  6       Record name   "DBREF "
///  8 - 11       IDcode        idCode             ID code of this entry.
/// 13            Character     chainID            Chain  identifier.
/// 15 - 18       Integer       seqBegin           Initial sequence number of the PDB sequence segment.
/// 19            AChar         insertBegin        Initial  insertion code of the PDB  sequence segment.
/// 21 - 24       Integer       seqEnd             Ending sequence number of the PDB  sequence segment.
/// 25            AChar         insertEnd          Ending insertion code of the PDB  sequence segment.
/// 27 - 32       LString       database           Sequence database name.
/// 34 - 41       LString       dbAccession        Sequence database accession code.
/// 43 - 54       LString       dbIdCode           Sequence  database identification code.
/// 56 - 60       Integer       dbseqBegin         Initial sequence number of the database seqment.
/// 61            AChar         idbnsBeg           Insertion code of initial residue of the segment, if PDB is the reference.
/// 63 - 67       Integer       dbseqEnd           Ending sequence number of the database segment.
/// 68            AChar         dbinsEnd           Insertion code of the ending residue of the segment, if PDB is the reference.
/// </summary>
public class DbRefRecord : AbstractChainedRecord
{
    public override string Tag => "DBREF";
    public override RecordType Type => RecordType.DbRef;

    public string? IdCode { get; set; }
    public int SeqBegin { get; set; }
    public char InsertBegin { get; set; }
    public int SeqEnd { get; set; }
    public char InsertEnd { get; set; }
    public string? Database { get; set; }
    public string? DbAccession { get; set; }
    public string? DbIdCode { get; set; }
    public int DbSeqBegin { get; set; }
    public char IdbnsBeg { get; set; }
    public int DbSeqEnd { get; set; }
    public char DbInsEnd { get; set; }

    public override void FromRow(string row)
    {
        IdCode = row.Substring(7, 4).Trim();          //  8 - 11
        ChainId = row[12];                            // 13
        SeqBegin = int.Parse(row.Substring(14, 4));   // 15 - 18
        InsertBegin = row[18];                        // 19
        SeqEnd = int.Parse(row.Substring(20, 4));     // 21 - 24
        InsertEnd = row[24];                          // 25
        Database = row.Substring(26, 6).Trim();       // 27 - 32
        DbAccession = row.Substring(33, 8).Trim();    // 34 - 41
        DbIdCode = row.Substring(42, 12).Trim();      // 43 - 54
        DbSeqBegin = int.Parse(row.Substring(55, 5)); // 56 - 60
        IdbnsBeg = row[60];                           // 61
        DbSeqEnd = int.Parse(row.Substring(62, 5));   // 63 - 67
        DbInsEnd = row[67];                           // 68
    }

    public override string ToRow()
    {
        return $"{Tag,-6} {IdCode,4} {ChainId,1} {SeqBegin,4}{InsertBegin,1} "                      //  1 - 20
            + $"{SeqEnd,4}{InsertEnd,1} {Database,-6} {DbAccession,-8} {DbIdCode,-12} {DbSeqBegin,5}"  // 21 - 60
            + $"{IdbnsBeg,1} {DbSeqEnd,5}{DbInsEnd,1}{"",12}";                                      // 61 - 80
    }

    public static DbRefRecord? CreateFromRow(string row)
    {
        DbRefRecord record = new();

        if (row[..6].Trim() != record.Tag)
            return null;

        record.FromRow(row);
        return record;
    }
}

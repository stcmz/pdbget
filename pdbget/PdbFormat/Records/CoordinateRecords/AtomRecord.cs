namespace pdbget.PdbFormat
{
    /// <summary>
    /// The ATOM records present the atomic coordinates for standard amino acids and nucleotides. They also present the occupancy and
    /// temperature factor for each atom. Non-polymer chemical coordinates use the HETATM record type. The element symbol is always
    /// present on each ATOM record; charge is optional. See
    /// https://www.wwpdb.org/documentation/file-format-content/format33/sect9.html#ATOM
    /// 
    /// COLUMNS        DATA  TYPE    FIELD        DEFINITION
    /// -------------------------------------------------------------------------------------
    ///  1 -  6        Record name   "ATOM  "
    ///  7 - 11        Integer       serial       Atom  serial number.
    /// 13 - 16        Atom          name         Atom name.
    /// 17             Character     altLoc       Alternate location indicator.
    /// 18 - 20        Residue name  resName      Residue name.
    /// 22             Character     chainID      Chain identifier.
    /// 23 - 26        Integer       resSeq       Residue sequence number.
    /// 27             AChar         iCode        Code for insertion of residues.
    /// 31 - 38        Real(8.3)     x            Orthogonal coordinates for X in Angstroms.
    /// 39 - 46        Real(8.3)     y            Orthogonal coordinates for Y in Angstroms.
    /// 47 - 54        Real(8.3)     z            Orthogonal coordinates for Z in Angstroms.
    /// 55 - 60        Real(6.2)     occupancy    Occupancy.
    /// 61 - 66        Real(6.2)     tempFactor   Temperature  factor.
    /// 77 - 78        LString(2)    element      Element symbol, right-justified.
    /// 79 - 80        LString(2)    charge       Charge  on the atom.
    /// </summary>
    public class AtomRecord : AbstractAtomRecord
    {
        public override string Tag => "ATOM";
        public override RecordType Type => RecordType.Atom;

        public static AtomRecord? CreateFromRow(string row)
        {
            var record = new AtomRecord();

            if (row.Substring(0, 6).Trim() != record.Tag)
                return null;

            record.FromRow(row);
            return record;
        }
    }
}

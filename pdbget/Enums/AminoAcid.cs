namespace pdbget
{
    public enum AminoAcid
    {
        // Charged (side chains often make salt bridges)
        [Match("Arg", "ARG", "R"), Name("Arg", InternalName = "R")]
        Arginine,
        [Match("Lys", "LYS", "K"), Name("Lys", InternalName = "K")]
        Lysine,
        [Match("Asp", "ASP", "D"), Name("Asp", InternalName = "D")]
        AsparticAcid,
        [Match("Glu", "GLU", "E"), Name("Glu", InternalName = "E")]
        GlutamicAcid,

        // Polar (usually participate in hydrogen bonds as proton donors or acceptors)
        [Match("Gln", "GLN", "Q"), Name("Gln", InternalName = "Q")]
        Glutamine,
        [Match("Asn", "ASN", "N"), Name("Asn", InternalName = "N")]
        Asparagine,
        [Match("His", "HIS", "H"), Name("His", InternalName = "H")]
        Histidine,
        [Match("Ser", "SER", "S"), Name("Ser", InternalName = "S")]
        Serine,
        [Match("Thr", "THR", "T"), Name("Thr", InternalName = "T")]
        Threonine,
        [Match("Tyr", "TYR", "Y"), Name("Tyr", InternalName = "Y")]
        Tyrosine,
        [Match("Cys", "CYS", "C"), Name("Cys", InternalName = "C")]
        Cysteine,
        [Match("Trp", "TRP", "W"), Name("Trp", InternalName = "W")]
        Tryptophan,

        // Hydrophobic (normally buried inside the protein core)
        [Match("Ala", "ALA", "A"), Name("Ala", InternalName = "A")]
        Alanine,
        [Match("Ile", "ILE", "I"), Name("Ile", InternalName = "I")]
        Isoleucine,
        [Match("Leu", "LEU", "L"), Name("Leu", InternalName = "L")]
        Leucine,
        [Match("Met", "MET", "M"), Name("Met", InternalName = "M")]
        Methionine,
        [Match("Phe", "PHE", "F"), Name("Phe", InternalName = "F")]
        Phenylalanine,
        [Match("Val", "VAL", "V"), Name("Val", InternalName = "V")]
        Valine,
        [Match("Pro", "PRO", "P"), Name("Pro", InternalName = "P")]
        Proline,
        [Match("Gly", "GLY", "G"), Name("Gly", InternalName = "G")]
        Glycine,
    }
}

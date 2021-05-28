namespace pdbget.PdbFormat
{
    public enum RecordType
    {
        // Title Section
        [Match("HEADER")]
        Header,

        [Match("OBSLTE")]
        Obsoleted,

        [Match("TITLE")]
        Title,

        [Match("SPLIT")]
        Split,

        [Match("CAVEAT")]
        Caveat,

        [Match("COMPND")]
        Compound,

        [Match("SOURCE")]
        Source,

        [Match("KEYWDS")]
        Keywords,

        [Match("EXPDTA")]
        ExperimentData,

        [Match("NUMMDL")]
        NumModels,

        [Match("MDLTYP")]
        ModelType,

        [Match("AUTHOR")]
        Author,

        [Match("REVDAT")]
        RevisionData,

        [Match("SPRSDE")]
        Sprsde,

        [Match("JRNL")]
        Journal,

        [Match("REMARK")]
        Remark,

        // Primary Structure Section
        [Match("DBREF")]
        DbRef, // standard format

        [Match("DBREF1")]
        DbRef1,

        [Match("DBREF2")]
        DbRef2,

        [Match("SEQADV")]
        SeqAdv,

        [Match("SEQRES")]
        SeqRes,

        [Match("MODRES")]
        ModRes,

        // Heterogen Section
        [Match("HET")]
        Heterogen,

        [Match("FORMUL")]
        Formula,

        [Match("HETNAM")]
        HeterogenName,

        [Match("HETSYN")]
        HeterogenSynonym,

        // Secondary Structure Section
        [Match("HELIX")]
        Helix,

        [Match("SHEET")]
        Sheet,

        // Connectivity Annotation Section
        [Match("SSBOND")]
        Ssbond,

        [Match("LINK")]
        Link,

        [Match("CISPEP")]
        Cispep,

        // Miscellaneous Features Section
        [Match("SITE")]
        Site,

        // Crystallographic and Coordinate Transformation Section
        [Match("CRYST1")]
        Crystal,

        [Match("MTRIX[123]", UseRegex = true)]
        Matrix,

        [Match("ORIGX[123]", UseRegex = true)]
        Origx,

        [Match("SCALE[123]", UseRegex = true)]
        Scale,

        // Coordinate Section
        [Match("MODEL")]
        Model,

        [Match("ATOM")]
        Atom,

        [Match("ANISOU")]
        Anisou,

        [Match("TER")]
        Terminated,

        [Match("HETATM")]
        HeteroAtom,

        [Match("ENDMDL")]
        EndModel,

        // Connectivity Section
        [Match("CONECT")]
        Conect,

        // Bookkeeping Section
        [Match("MASTER")]
        Master,

        [Match("END")]
        End,

        // Unknown Types
        [Match("SIGATM")]
        SigAtm,

        [Match("SIGUIJ")]
        SigUij,

        [Match("HYDBND")]
        HydBnd,

        [Match("FTNOTE")]
        FootNote,

        [Match("SLTBRG")]
        SltBrg,

        [Match("TURN")]
        Turn,
    }
}

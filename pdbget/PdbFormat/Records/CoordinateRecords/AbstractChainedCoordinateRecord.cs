namespace pdbget.PdbFormat
{
    public abstract class AbstractChainedCoordinateRecord : AbstractChainedRecord
    {
        public int Serial { get; set; }
        public string? ResidueName { get; set; }
        public int ResidueSequence { get; set; }
        public char InsertionCode { get; set; }
    }
}

namespace DNNE.Language.Clarion
{
    internal struct ExportQ
    {
        public string Symbol { get; init; }
        public long Ordinal { get; init; }
        public string Module { get; init; }
        public long OrgOrder { get; init; }
        public short TreeLevel { get; init; }
    }
}

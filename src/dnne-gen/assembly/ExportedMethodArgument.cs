using System.Collections.Immutable;

namespace DNNE.Assembly
{
    public struct ExportedMethodArgument
    {
        public int Index { get; init; }
        public string Type { get; init; }
        public string Name { get; init; }
        public bool HasDefault { get; init; }
        public ImmutableList<UsedAttribute> Attributes { get; set; }
    }
}
using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace DNNE.Assembly.Old
{
    [DataContract]
    public struct ExportedMethodArgument
    {
        [DataMember]
        public int Index { get; init; }
        [DataMember]
        public string Type { get; init; }
        [DataMember]
        public string Name { get; init; }
        [DataMember]
        public bool HasDefault { get; init; }
        [DataMember]
        public ImmutableList<UsedAttribute> Attributes { get; set; }
    }
}
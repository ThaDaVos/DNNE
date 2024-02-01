
using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace DNNE.Assembly.Old
{
    [DataContract]
    public struct AssemblyInformation
    {
        [DataMember]
        public string Name { get; init; }
        [DataMember]
        public string SafeName => Name.Replace("_", "").Replace(".", "_");
        [DataMember]
        public string Path { get; init; }
        [DataMember]
        public ImmutableList<ExportedType> ExportedTypes { get; init; }
        [DataMember]
        public ImmutableList<string> AdditionalStatements { get; init; }
    }
}
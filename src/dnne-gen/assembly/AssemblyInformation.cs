
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DNNE.Assembly
{
    [DataContract]
    public struct AssemblyInformation
    {
        [DataMember]
        public string Name { get; init; }
        [DataMember]
        public string Path { get; init; }
        [DataMember]
        public ImmutableList<ExportedType> ExportedTypes { get; init; }
        [DataMember]
        public ImmutableList<ExportedMethod> ExportedMethods { get; init; }
        [DataMember]
        public ImmutableList<string> AdditionalStatements { get; init; }
    }
}
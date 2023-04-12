
using System.Collections.Immutable;
using System.Xml.Serialization;

namespace DNNE.Assembly
{
    public struct AssemblyInformation
    {
        public string Name { get; init; }
        public string Path { get; init; }
        public ImmutableList<ExportedType> ExportedTypes { get; init; }
        [XmlIgnore]
        public ImmutableList<ExportedMethod> ExportedMethods { get; init; }
        public ImmutableList<string> AdditionalStatements { get; init; }
    }
}
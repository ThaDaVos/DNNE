using System.Collections.Generic;

namespace DNNE
{
    internal struct AssemblyInformation
    {
        public string Name { get; init; }
        public string Path { get; init; }
        public IEnumerable<ExportedType> ExportedTypes { get; init; }
        public IEnumerable<ExportedMethod> ExportedMethods { get; init; }
        public IEnumerable<string> AdditionalStatements { get; init; }
    }
}
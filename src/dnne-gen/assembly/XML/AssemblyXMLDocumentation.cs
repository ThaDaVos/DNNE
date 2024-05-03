using System.Collections.Generic;

namespace DNNE.Assembly.XML;

internal record class AssemblyXMLDocumentation
{
    public required string Name { get; init; }
    public required IEnumerable<AssemblyXMLDocumentationMember> Members { get; init; }
}

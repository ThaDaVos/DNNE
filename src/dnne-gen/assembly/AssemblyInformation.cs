using DNNE.Assembly.Entities.Interfaces;
using DNNE.Assembly.XML;

namespace DNNE.Assembly;

internal record AssemblyInformation
{
    public required IExportedAssembly Assembly { get; init; }
    public required AssemblyXMLDocumentation? Documentation { get; init; }
}

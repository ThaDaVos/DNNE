using DNNE.Assembly.Entities;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Source.Naming;

internal interface INamingHelper
{
    string Delimiter { get; }
    string ResolveFieldName(ExportedField field);
    string ResolveMethodName(IExportedMethod method);
    string ResolveNestedTypeName(IExportedType nestedType);
    string ResolvePropertyName(ExportedProperty property);
    string ResolveTypeName(IExportedType type);
    INamingHelper WithDelimiter(string delimiter);
}

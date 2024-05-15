using System;
using DNNE.Assembly.Entities;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Source.Naming;

internal record class DefaultNamingHelper : INamingHelper
{
    private string delimiter;
    public string Delimiter => delimiter;
    public DefaultNamingHelper(string delimiter = "_")
    {
        this.delimiter = delimiter;
    }

    public string ResolveFieldName(ExportedField field)
    {
        if (field.Parent is IExportedType type)
        {
            return ResolveTypeName(type) + Delimiter + field.Name;
        }

        throw new NotSupportedException();
    }

    public string ResolveMethodName(IExportedMethod method)
    {
        if (method.Parent is IExportedType type)
        {
            return ResolveTypeName(type) + Delimiter + method.Name;
        }

        throw new NotSupportedException();
    }

    public string ResolveNestedTypeName(IExportedType nestedType)
    {
        if (nestedType.Parent is IExportedType type)
        {
            return ResolveTypeName(type) + Delimiter + nestedType.Name;
        }

        throw new NotSupportedException();
    }

    public string ResolvePropertyName(ExportedProperty property)
    {
        if (property.Parent is IExportedType type)
        {
            return ResolveTypeName(type) + Delimiter + property.Name;
        }

        throw new NotSupportedException();
    }

    public string ResolveTypeName(IExportedType type)
    {
        if (type.Parent is IExportedType parentType)
        {
            return ResolveTypeName(parentType) + Delimiter + type.Name;
        }

        if (type.Parent is IExportedAssembly assembly)
        {
            return assembly.Name + Delimiter + type.Name;
        }

        return type.Name;
    }

    public INamingHelper WithDelimiter(string delimiter) => this with
    {
        delimiter = delimiter
    };
}

using System;
using System.Reflection;
using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities;

internal class NamedArgumentOfExportedAttribute : ExportedValuedEntity<CustomAttributeNamedArgument<string>>
{
    public NamedArgumentOfExportedAttribute(MetadataReader metadataReader, CustomAttributeNamedArgument<string> entity, IExportedEntity? parent = null) : base(metadataReader, entity, parent)
    {
    }

    protected override string GetName() => entity.Name ?? throw new InvalidOperationException("Name is not set.");
    protected override dynamic? GetValue() => entity.Value;
}

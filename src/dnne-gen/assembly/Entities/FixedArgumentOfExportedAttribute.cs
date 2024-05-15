using System;
using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities;

internal class FixedArgumentOfExportedAttribute : ExportedValuedEntity<CustomAttributeTypedArgument<string>>
{
    private readonly string _name;

    public FixedArgumentOfExportedAttribute(MetadataReader metadataReader, CustomAttributeTypedArgument<string> entity, string name, IExportedEntity? parent = null) : base(metadataReader, entity, parent)
    {
        _name = name;
    }

    protected override string GetName() => _name ?? throw new InvalidOperationException("Name is not set.");

    protected override dynamic? GetValue() => entity.Value;
}

using System;
using System.Reflection.Metadata;

namespace DNNE.Assembly.Entities;

internal class FixedArgumentOfExportedAttribute : ExportedValuedEntity<CustomAttributeTypedArgument<string>>
{
    private readonly string _name;

    public FixedArgumentOfExportedAttribute(MetadataReader metadataReader, CustomAttributeTypedArgument<string> entity, string name) : base(metadataReader, entity)
    {
        _name = name;
    }

    protected override string GetName() => _name ?? throw new InvalidOperationException("Name is not set.");

    protected override dynamic? GetValue() => entity.Value;
}

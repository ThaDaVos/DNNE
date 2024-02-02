using System.Collections.Generic;
using System.Reflection.Metadata;
using DNNE.Assembly;

namespace DNNE;

internal class ExportedType : ExportedEntity<TypeDefinition>
{
    private IEnumerable<ExportedMethod>? methods;
    private IEnumerable<ExportedProperty>? properties;
    private IEnumerable<ExportedField>? fields;

    public IEnumerable<ExportedMethod> Methods => methods ??= GetMethods();
    public IEnumerable<ExportedProperty> Properties => properties ??= GetProperties();
    public IEnumerable<ExportedField> Fields => fields ??= GetFields();

    public ExportedType(MetadataReader metadataReader, TypeDefinition definition) : base(metadataReader, definition)
    {
    }

    protected override CustomAttributeHandleCollection GetCustomAttributeHandles() => definition.GetCustomAttributes();

    protected override string GetName() => metadataReader.GetString(definition.Name);

    internal IEnumerable<ExportedMethod> GetMethods() => definition.GetExportedMethods(metadataReader);
    internal IEnumerable<ExportedProperty> GetProperties() => definition.GetExportedProperties(metadataReader);
    internal IEnumerable<ExportedField> GetFields() => definition.GetExportedFields(metadataReader);
}

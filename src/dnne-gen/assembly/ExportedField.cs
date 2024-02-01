using System.Reflection.Metadata;

namespace DNNE.Assembly;

internal class ExportedField : ExportedValuedEntity<FieldDefinition>
{
    public ExportedField(MetadataReader metadataReader, FieldDefinition definition) : base(metadataReader, definition)
    {
    }
    protected override string GetName() => metadataReader.GetString(definition.Name);
    protected override ConstantHandle GetConstantHandle() => definition.GetDefaultValue();
    protected override CustomAttributeHandleCollection GetCustomAttributeHandles() => definition.GetCustomAttributes();
}

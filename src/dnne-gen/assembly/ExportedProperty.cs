using System.Reflection.Metadata;

namespace DNNE.Assembly;

internal class ExportedProperty : ExportedValuedEntity<PropertyDefinition>
{
    public ExportedProperty(MetadataReader metadataReader, PropertyDefinition definition) : base(metadataReader, definition)
    {
    }
    protected override string GetName() => metadataReader.GetString(definition.Name);
    protected override ConstantHandle GetConstantHandle() => definition.GetDefaultValue();
    protected override CustomAttributeHandleCollection GetCustomAttributeHandles() => definition.GetCustomAttributes();
}

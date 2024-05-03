using System.Reflection.Metadata;

namespace DNNE.Assembly.Entities;

internal class ExportedProperty : ExportedAttributedConstantValuedEntity<PropertyDefinition>
{
    public ExportedProperty(MetadataReader metadataReader, PropertyDefinition entity) : base(metadataReader, entity)
    {
    }
    protected override string GetName() => metadataReader.GetString(entity.Name);
    protected override ConstantHandle GetConstantHandle() => entity.GetDefaultValue();
    protected override CustomAttributeHandleCollection GetCustomAttributeHandles() => entity.GetCustomAttributes();
}

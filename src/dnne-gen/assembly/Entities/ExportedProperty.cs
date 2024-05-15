using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities;

internal class ExportedProperty : ExportedAttributedConstantValuedEntity<PropertyDefinition>
{
    public ExportedProperty(MetadataReader metadataReader, PropertyDefinition entity, IExportedEntity? parent = null) : base(metadataReader, entity, parent)
    {
    }
    protected override string GetName() => metadataReader.GetString(entity.Name);
    protected override ConstantHandle GetConstantHandle() => entity.GetDefaultValue();
    protected override CustomAttributeHandleCollection GetCustomAttributeHandles() => entity.GetCustomAttributes();
}

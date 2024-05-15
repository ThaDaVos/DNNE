using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities;

internal class ExportedMethodParameter : ExportedAttributedConstantValuedEntity<Parameter>
{
    private string type;
    public override string Type => type;
    public int SequenceNumber => entity.SequenceNumber;

    public ExportedMethodParameter(MetadataReader metadataReader, Parameter entity, string type, IExportedEntity? parent = null) : base(metadataReader, entity, parent)
    {
        this.type = type;
    }

    protected override string GetName() => metadataReader.GetString(entity.Name);

    protected override CustomAttributeHandleCollection GetCustomAttributeHandles()
        => entity.GetCustomAttributes();

    protected override ConstantHandle GetConstantHandle()
        => entity.GetDefaultValue();
}

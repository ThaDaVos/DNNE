using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities;

internal abstract class ExportedValuedEntity<TEntity> : ExportedEntity<TEntity>, IExportedValuedEntity where TEntity : struct
{
    private dynamic? value;
    public dynamic? Value
    {
        get => value ??= GetValue();
    }
    public virtual string Type => (Value?.GetType() ?? typeof(object)).ToString();
    public bool IsNil => Value == null;
    protected ExportedValuedEntity(MetadataReader metadataReader, TEntity entity, IExportedEntity? parent = null) : base(metadataReader, entity, parent)
    {
    }
    protected abstract dynamic? GetValue();
}

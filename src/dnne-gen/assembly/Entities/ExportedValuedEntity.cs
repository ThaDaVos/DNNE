using System;
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
    public Type Type => Value?.GetType() ?? typeof(object);
    public bool IsNil => Value == null;
    protected ExportedValuedEntity(MetadataReader metadataReader, TEntity entity) : base(metadataReader, entity)
    {
    }
    protected abstract dynamic? GetValue();
}

using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities;

internal abstract class ExportedEntity<TEntity> : IExportedEntity where TEntity : struct
{
    protected readonly MetadataReader metadataReader;
    protected readonly TEntity entity;
    protected string? name;
    public string Name => name ??= GetName();
    protected IExportedEntity? parent;
    public IExportedEntity? Parent => parent;

    public ExportedEntity(MetadataReader metadataReader, TEntity entity, IExportedEntity? parent = null)
    {
        this.metadataReader = metadataReader;
        this.entity = entity;
        this.parent = parent;
    }
    protected abstract string GetName();
}

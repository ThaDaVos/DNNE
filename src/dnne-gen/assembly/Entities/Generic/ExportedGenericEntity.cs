using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities.Generic;

internal abstract class ExportedGenericEntity<TEntity> : ExportedEntity<TEntity> where TEntity : struct
{
    protected readonly GenericParametersContext genericParametersContext;

    protected ExportedGenericEntity(MetadataReader metadataReader, TEntity entity, IExportedEntity? parent = null) : base(metadataReader, entity, parent)
    {
        this.genericParametersContext = GetGenericParametersContext(metadataReader, entity);
    }

    protected abstract GenericParametersContext GetGenericParametersContext(MetadataReader metadataReader, TEntity entity);
}

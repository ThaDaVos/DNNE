using System.Reflection.Metadata;

namespace DNNE.Assembly.Entities.Generic;

internal abstract class ExportedGenericEntity<TEntity> : ExportedEntity<TEntity> where TEntity : struct
{
    protected readonly GenericParametersContext genericParametersContext;

    protected ExportedGenericEntity(MetadataReader metadataReader, TEntity entity) : base(metadataReader, entity)
    {
        this.genericParametersContext = GetGenericParametersContext(metadataReader, entity);
    }

    protected abstract GenericParametersContext GetGenericParametersContext(MetadataReader metadataReader, TEntity entity);
}

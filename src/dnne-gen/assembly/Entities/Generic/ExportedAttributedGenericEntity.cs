using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities.Generic;

internal abstract class ExportedAttributedGenericEntity<TEntity> : ExportedGenericEntity<TEntity>, IExportedAttributedEntity where TEntity : struct
{
    protected ExportedAttributedGenericEntity(MetadataReader metadataReader, TEntity entity) : base(metadataReader, entity)
    {
    }

    public IEnumerable<IExportedAttribute> CustomAttributes => GetCustomAttributes();
    protected abstract CustomAttributeHandleCollection GetCustomAttributeHandles();
    internal IEnumerable<IExportedAttribute> GetCustomAttributes() => GetCustomAttributeHandles()
            .Select(
                (CustomAttributeHandle handle) => new ExportedAttribute(metadataReader, handle)
            );
}

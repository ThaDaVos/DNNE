using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities;

internal abstract class ExportedAttributedConstantValuedEntity<TEntity> : ExportedConstantValuedEntity<TEntity>, IExportedConstantValuedEntity, IExportedAttributedEntity where TEntity : struct
{
    protected ExportedAttributedConstantValuedEntity(MetadataReader metadataReader, TEntity entity, IExportedEntity? parent = null) : base(metadataReader, entity, parent)
    {
    }

    public IEnumerable<IExportedAttribute> CustomAttributes => GetCustomAttributes();
    protected abstract CustomAttributeHandleCollection GetCustomAttributeHandles();
    internal IEnumerable<IExportedAttribute> GetCustomAttributes() => GetCustomAttributeHandles()
            .Select(
                (CustomAttributeHandle handle) => new ExportedAttribute(metadataReader, handle, this)
            );
}

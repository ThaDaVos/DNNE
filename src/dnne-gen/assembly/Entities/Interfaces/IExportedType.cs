using System.Collections.Generic;

namespace DNNE.Assembly.Entities.Interfaces;

public interface IExportedType : IExportedAttributedEntity
{
    internal IEnumerable<IExportedMethod> Methods { get; }
    internal IEnumerable<ExportedProperty> Properties { get; }
    internal IEnumerable<ExportedField> Fields { get; }
    internal IEnumerable<IExportedType> NestedExportedTypes { get; }
}

using System.Collections.Generic;

namespace DNNE.Assembly.Entities.Interfaces;

public interface IExportedAttributedEntity : IExportedEntity
{
    internal IEnumerable<IExportedAttribute> CustomAttributes { get; }
}

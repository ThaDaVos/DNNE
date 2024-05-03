using System.Collections.Generic;

namespace DNNE.Assembly.Entities.Interfaces;

public interface IExportedAssembly : IExportedEntity
{
    internal string SafeName { get; }
    internal string Path { get; }
    internal IEnumerable<IExportedType> ExportedTypes { get; }
}

using System.Collections.Generic;
using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities;

internal class ExportedAssembly : ExportedEntity<AssemblyDefinition>, IExportedAssembly
{
    private string _path;
    public ExportedAssembly(MetadataReader metadataReader, AssemblyDefinition entity, string path) : base(metadataReader, entity)
    {
        _path = path;
    }

    public string SafeName => Name.Replace("_", "").Replace(".", "_");
    public string Path => _path;
    public IEnumerable<IExportedType> ExportedTypes => metadataReader.GetExportedTypes();
    protected override string GetName() => metadataReader.GetString(entity.Name);
}

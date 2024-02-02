using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;

namespace DNNE.Assembly;

internal abstract class ExportedEntity<TDefinition> where TDefinition : struct
{
    protected readonly MetadataReader metadataReader;
    protected readonly TDefinition definition;
    private string? name;
    internal string Name => name ??= GetName();
    public ExportedEntity(MetadataReader metadataReader, TDefinition definition)
    {
        this.definition = definition;
        this.metadataReader = metadataReader;
    }
    protected abstract string GetName();
    protected abstract CustomAttributeHandleCollection GetCustomAttributeHandles();
    internal ImmutableArray<UsedAttribute<TType>> GetCustomAttributes<TType>(ICustomAttributeTypeProvider<TType> customAttributeTypeProvider) => GetCustomAttributeHandles()
            .Select(
                handle => new UsedAttribute<TType>(
                    metadataReader,
                    metadataReader.GetCustomAttribute(handle),
                    customAttributeTypeProvider
                )
            )
            .ToImmutableArray();
}

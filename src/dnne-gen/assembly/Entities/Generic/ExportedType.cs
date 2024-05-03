using System.Collections.Generic;
using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities.Generic;

internal class ExportedType : ExportedAttributedGenericEntity<TypeDefinition>, IExportedType
{
    private IEnumerable<IExportedMethod>? methods;
    private IEnumerable<ExportedProperty>? properties;
    private IEnumerable<ExportedField>? fields;
    private IEnumerable<IExportedType>? nestedExportedTypes;

    public IEnumerable<IExportedMethod> Methods => methods ??= GetMethods();
    public IEnumerable<ExportedProperty> Properties => properties ??= GetProperties();
    public IEnumerable<ExportedField> Fields => fields ??= GetFields();
    public IEnumerable<IExportedType> NestedExportedTypes => nestedExportedTypes ??= GetNestedExportedTypes();

    public ExportedType(MetadataReader metadataReader, TypeDefinition entity) : base(metadataReader, entity)
    {
    }

    protected override string GetName() => metadataReader.GetString(entity.Name);
    protected override CustomAttributeHandleCollection GetCustomAttributeHandles() => entity.GetCustomAttributes();
    internal IEnumerable<IExportedMethod> GetMethods() => entity.GetExportedMethods(metadataReader);
    internal IEnumerable<ExportedProperty> GetProperties() => entity.GetExportedProperties(metadataReader);
    internal IEnumerable<ExportedField> GetFields() => entity.GetExportedFields(metadataReader);
    internal IEnumerable<IExportedType> GetNestedExportedTypes()
    {
        foreach (TypeDefinitionHandle nestedTypeHandle in entity.GetNestedTypes())
        {
            yield return metadataReader.ToExportedEntity(nestedTypeHandle);
        }

        yield break;
    }

    protected override GenericParametersContext GetGenericParametersContext(MetadataReader metadataReader, TypeDefinition entity)
        => new GenericParametersContext(metadataReader, entity.GetGenericParameters());
}

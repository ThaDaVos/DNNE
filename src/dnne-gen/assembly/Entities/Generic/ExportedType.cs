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

    public ExportedType(MetadataReader metadataReader, TypeDefinition entity, IExportedEntity? parent = null) : base(metadataReader, entity, parent)
    {
    }

    protected override string GetName() => metadataReader.GetString(entity.Name);
    protected override CustomAttributeHandleCollection GetCustomAttributeHandles() => entity.GetCustomAttributes();
    internal IEnumerable<IExportedMethod> GetMethods() => entity.GetExportedMethods(metadataReader, this);
    internal IEnumerable<ExportedProperty> GetProperties() => entity.GetExportedProperties(metadataReader, this);
    internal IEnumerable<ExportedField> GetFields() => entity.GetExportedFields(metadataReader, this);
    internal IEnumerable<IExportedType> GetNestedExportedTypes()
    {
        foreach (TypeDefinitionHandle nestedTypeHandle in entity.GetNestedTypes())
        {
            yield return metadataReader.ToExportedEntity(nestedTypeHandle, this);
        }

        yield break;
    }

    protected override GenericParametersContext GetGenericParametersContext(MetadataReader metadataReader, TypeDefinition entity)
        => new GenericParametersContext(metadataReader, entity.GetGenericParameters());
}

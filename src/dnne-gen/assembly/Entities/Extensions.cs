using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities;

internal static class Extensions
{
    internal static IExportedType ToExportedEntity(this MetadataReader metadataReader, TypeDefinitionHandle? definitionHandle)
    {
        if (metadataReader == null)
        {
            throw new ArgumentNullException(nameof(metadataReader));
        }

        if (definitionHandle == null)
        {
            throw new ArgumentNullException(nameof(definitionHandle));
        }

        TypeDefinition entity = metadataReader.GetTypeDefinition(definitionHandle.Value);

        return entity.GetGenericParameters().Count > 0
            ? new Generic.ExportedType(metadataReader, entity)
            : new ExportedType(metadataReader, entity);
    }

    internal static IEnumerable<IExportedType> GetExportedTypes(this MetadataReader metadataReader) => metadataReader
            .TypeDefinitions
            .Select(definitionHandle => metadataReader.ToExportedEntity(definitionHandle));

    internal static ExportedField ToExportedEntity(this MetadataReader metadataReader, FieldDefinitionHandle? definitionHandle)
    {
        if (metadataReader == null)
        {
            throw new ArgumentNullException(nameof(metadataReader));
        }

        if (definitionHandle == null)
        {
            throw new ArgumentNullException(nameof(definitionHandle));
        }

        return new(metadataReader, metadataReader.GetFieldDefinition(definitionHandle.Value));
    }

    internal static IEnumerable<ExportedField> GetExportedFields(this TypeDefinition typeDefinition, MetadataReader metadataReader) => typeDefinition
            .GetFields()
            .Select(definitionHandle => metadataReader.ToExportedEntity(definitionHandle));

    internal static IExportedMethod ToExportedEntity(this MetadataReader metadataReader, MethodDefinitionHandle? definitionHandle)
    {
        if (metadataReader == null)
        {
            throw new ArgumentNullException(nameof(metadataReader));
        }

        if (definitionHandle == null)
        {
            throw new ArgumentNullException(nameof(definitionHandle));
        }

        MethodDefinition entity = metadataReader.GetMethodDefinition(definitionHandle.Value);

        return entity.GetGenericParameters().Count > 0
            ? new Generic.ExportedMethod(metadataReader, entity)
            : new ExportedMethod(metadataReader, entity);
    }

    internal static IEnumerable<IExportedMethod> GetExportedMethods(this TypeDefinition typeDefinition, MetadataReader metadataReader)
    {
        return typeDefinition
            .GetMethods()
            .Select(definitionHandle => metadataReader.ToExportedEntity(definitionHandle));
    }

    internal static ExportedProperty ToExportedEntity(this MetadataReader metadataReader, PropertyDefinitionHandle? definitionHandle)
    {
        if (metadataReader == null)
        {
            throw new ArgumentNullException(nameof(metadataReader));
        }

        if (definitionHandle == null)
        {
            throw new ArgumentNullException(nameof(definitionHandle));
        }

        return new(metadataReader, metadataReader.GetPropertyDefinition(definitionHandle.Value));
    }

    internal static IEnumerable<ExportedProperty> GetExportedProperties(this TypeDefinition typeDefinition, MetadataReader metadataReader) => typeDefinition
            .GetProperties()
            .Select(definitionHandle => metadataReader.ToExportedEntity(definitionHandle));

}

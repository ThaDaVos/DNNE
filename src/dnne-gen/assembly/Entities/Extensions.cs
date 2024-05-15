using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;

namespace DNNE.Assembly.Entities;

internal static class Extensions
{
    internal static IExportedType ToExportedEntity(this MetadataReader metadataReader, TypeDefinitionHandle? definitionHandle, IExportedEntity? parent = null)
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
            ? new Generic.ExportedType(metadataReader, entity, parent)
            : new ExportedType(metadataReader, entity, parent);
    }

    internal static IEnumerable<IExportedType> GetExportedTypes(this MetadataReader metadataReader, IExportedEntity? parent = null) => metadataReader
            .TypeDefinitions
            .Select(definitionHandle => metadataReader.ToExportedEntity(definitionHandle, parent));

    internal static ExportedField ToExportedEntity(this MetadataReader metadataReader, FieldDefinitionHandle? definitionHandle, IExportedEntity? parent = null)
    {
        if (metadataReader == null)
        {
            throw new ArgumentNullException(nameof(metadataReader));
        }

        if (definitionHandle == null)
        {
            throw new ArgumentNullException(nameof(definitionHandle));
        }

        return new(metadataReader, metadataReader.GetFieldDefinition(definitionHandle.Value), parent);
    }

    internal static IEnumerable<ExportedField> GetExportedFields(this TypeDefinition typeDefinition, MetadataReader metadataReader, IExportedEntity? parent = null) => typeDefinition
            .GetFields()
            .Select(definitionHandle => metadataReader.ToExportedEntity(definitionHandle, parent));

    internal static IExportedMethod ToExportedEntity(this MetadataReader metadataReader, MethodDefinitionHandle? definitionHandle, IExportedEntity? parent = null)
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
            ? new Generic.ExportedMethod(metadataReader, entity, parent)
            : new ExportedMethod(metadataReader, entity, parent);
    }

    internal static IEnumerable<IExportedMethod> GetExportedMethods(this TypeDefinition typeDefinition, MetadataReader metadataReader, IExportedEntity? parent = null) => typeDefinition
            .GetMethods()
            .Select(definitionHandle => metadataReader.ToExportedEntity(definitionHandle, parent));

    internal static ExportedProperty ToExportedEntity(this MetadataReader metadataReader, PropertyDefinitionHandle? definitionHandle, IExportedEntity? parent = null)
    {
        if (metadataReader == null)
        {
            throw new ArgumentNullException(nameof(metadataReader));
        }

        if (definitionHandle == null)
        {
            throw new ArgumentNullException(nameof(definitionHandle));
        }

        return new(metadataReader, metadataReader.GetPropertyDefinition(definitionHandle.Value), parent);
    }

    internal static IEnumerable<ExportedProperty> GetExportedProperties(this TypeDefinition typeDefinition, MetadataReader metadataReader, IExportedEntity? parent = null) => typeDefinition
            .GetProperties()
            .Select(definitionHandle => metadataReader.ToExportedEntity(definitionHandle, parent));

}

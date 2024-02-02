using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Metadata;
using DNNE.Assembly;

namespace DNNE;

internal static class Extensions
{
    internal static TSource AggregateSafely<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (func == null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        return source.Any() ? source.Aggregate(func) : default;
    }

    internal static ExportedType ToExportedEntity(this MetadataReader metadataReader, TypeDefinitionHandle? definitionHandle)
    {
        if (metadataReader == null)
        {
            throw new ArgumentNullException(nameof(metadataReader));
        }

        if (definitionHandle == null)
        {
            throw new ArgumentNullException(nameof(definitionHandle));
        }

        return new(metadataReader, metadataReader.GetTypeDefinition(definitionHandle.Value));
    }

    internal static IEnumerable<ExportedType> GetExportedTypes(this MetadataReader metadataReader) => metadataReader
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

    internal static ExportedMethod ToExportedEntity(this MetadataReader metadataReader, MethodDefinitionHandle? definitionHandle)
    {
        if (metadataReader == null)
        {
            throw new ArgumentNullException(nameof(metadataReader));
        }

        if (definitionHandle == null)
        {
            throw new ArgumentNullException(nameof(definitionHandle));
        }

        return new(metadataReader, metadataReader.GetMethodDefinition(definitionHandle.Value));
    }

    internal static IEnumerable<ExportedMethod> GetExportedMethods(this TypeDefinition typeDefinition, MetadataReader metadataReader)
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

    internal static bool ReadBoolean(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadBoolean();
    }
    internal static char ReadChar(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadChar();
    }
    internal static sbyte ReadSByte(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadSByte();
    }
    internal static byte ReadByte(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadByte();
    }
    internal static short ReadInt16(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadInt16();
    }
    internal static ushort ReadUInt16(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadUInt16();
    }
    internal static int ReadInt32(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadInt32();
    }
    internal static uint ReadUInt32(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadUInt32();
    }
    internal static long ReadInt64(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadInt64();
    }
    internal static ulong ReadUInt64(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadUInt64();
    }
    internal static float ReadSingle(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadSingle();
    }
    internal static double ReadDouble(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadDouble();
    }
    internal static string ReadUTF16(this BlobHandle blob, MetadataReader metadataReader)
    {
        BlobReader constantValueReader = metadataReader.GetBlobReader(blob);

        return constantValueReader.ReadUTF16(constantValueReader.Length);
    }

}
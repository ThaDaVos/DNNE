using System.Reflection.Metadata;

namespace DNNE.Assembly;

internal static class BlobHandleExtensions
{
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

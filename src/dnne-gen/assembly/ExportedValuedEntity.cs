using System;
using System.Reflection.Metadata;

namespace DNNE.Assembly;

internal abstract class ExportedValuedEntity<TDefinition> : ExportedEntity<TDefinition> where TDefinition : struct
{
    private Constant? constant;
    private dynamic? value;
    internal dynamic? Value
    {
        get => value ??= GetValue();
    }
    internal Type Type => Value?.GetType() ?? typeof(object);
    internal KnownType KnownType => GetKnownType() ?? KnownType.UNKNOWN;
    internal bool IsNil => Value == null;
    protected ExportedValuedEntity(MetadataReader metadataReader, TDefinition definition) : base(metadataReader, definition)
    {
    }
    protected abstract ConstantHandle GetConstantHandle();
    private Constant? GetConstant()
    {
        var constantHandle = GetConstantHandle();

        if (constantHandle.IsNil != false)
        {
            return null;
        }

        return metadataReader.GetConstant(constantHandle);
    }
    private KnownType? GetKnownType()
    {
        if (constant.HasValue == false)
        {
            constant = GetConstant();
        }

        return constant.HasValue ? constant.Value.TypeCode.ToKnownType() : KnownType.NULL;
    }
    private dynamic? GetValue()
    {
        if (constant.HasValue == false)
        {
            constant = GetConstant();
        }

        return constant.HasValue ? constant.Value.TypeCode switch
        {
            ConstantTypeCode.Boolean => constant.Value.Value.ReadBoolean(metadataReader),
            ConstantTypeCode.Char => constant.Value.Value.ReadChar(metadataReader),
            ConstantTypeCode.SByte => constant.Value.Value.ReadSByte(metadataReader),
            ConstantTypeCode.Byte => constant.Value.Value.ReadByte(metadataReader),
            ConstantTypeCode.Int16 => constant.Value.Value.ReadInt16(metadataReader),
            ConstantTypeCode.UInt16 => constant.Value.Value.ReadUInt16(metadataReader),
            ConstantTypeCode.Int32 => constant.Value.Value.ReadInt32(metadataReader),
            ConstantTypeCode.UInt32 => constant.Value.Value.ReadUInt32(metadataReader),
            ConstantTypeCode.Int64 => constant.Value.Value.ReadInt64(metadataReader),
            ConstantTypeCode.UInt64 => constant.Value.Value.ReadUInt64(metadataReader),
            ConstantTypeCode.Single => constant.Value.Value.ReadSingle(metadataReader),
            ConstantTypeCode.Double => constant.Value.Value.ReadDouble(metadataReader),
            ConstantTypeCode.String => constant.Value.Value.ReadUTF16(metadataReader),
            _ => null,
        } : null;
    }
}

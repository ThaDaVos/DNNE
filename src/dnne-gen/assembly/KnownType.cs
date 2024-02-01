using System;
using System.Reflection.Metadata;

namespace DNNE.Assembly;

/// <summary>
/// Based on https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/metadata/corelementtype-enumeration
/// </summary>
public enum KnownType
{
    /// <summary>
    /// Represents an unknown value.
    /// </summary>
    UNKNOWN = -1,
    /// <summary>
    /// Used internally.
    /// </summary>
    END = 0x0,
    /// <summary>
    /// A void type.
    /// </summary>
    VOID = 0x1,
    /// <summary>
    /// A Boolean type
    /// </summary>
    BOOLEAN = 0x2,
    /// <summary>
    /// A character type.
    /// </summary>
    CHAR = 0x3,
    /// <summary>
    /// A signed 1-byte integer.
    /// </summary>
    I1 = 0x4,
    /// <summary>
    /// An unsigned 1-byte integer.
    /// </summary>
    U1 = 0x5,
    /// <summary>
    /// A signed 2-byte integer.
    /// </summary>
    I2 = 0x6,
    /// <summary>
    /// An unsigned 2-byte integer.
    /// </summary>
    U2 = 0x7,
    /// <summary>
    /// A signed 4-byte integer.
    /// </summary>
    I4 = 0x8,
    /// <summary>
    /// An unsigned 4-byte integer.
    /// </summary>
    U4 = 0x9,
    /// <summary>
    /// A signed 8-byte integer.
    /// </summary>
    I8 = 0xa,
    /// <summary>
    /// An unsigned 8-byte integer.
    /// </summary>
    U8 = 0xb,
    /// <summary>
    /// A 4-byte floating point.
    /// </summary>
    R4 = 0xc,
    /// <summary>
    /// An 8-byte floating point.
    /// </summary>
    R8 = 0xd,
    /// <summary>
    /// A System.String type.
    /// </summary>
    STRING = 0xe,
    /// <summary>
    /// A pointer type modifier.
    /// </summary>
    PTR = 0xf,
    /// <summary>
    /// A unsigned pointer type modifier.
    /// </summary>
    UPTR = 0x23,
    /// <summary>
    /// A reference type modifier.
    /// </summary>
    BYREF = 0x10,
    /// <summary>
    /// A value type modifier.
    /// </summary>
    VALUETYPE = 0x11,
    /// <summary>
    /// A class type modifier.
    /// </summary>
    CLASS = 0x12,
    /// <summary>
    /// A class variable type modifier.
    /// </summary>
    VAR = 0x13,
    /// <summary>
    /// A multi-dimensional array type modifier.
    /// </summary>
    ARRAY = 0x14,
    /// <summary>
    /// A type modifier for generic types.
    /// </summary>
    GENERICINST = 0x15,
    /// <summary>
    /// A typed reference.
    /// </summary>
    TYPEDBYREF = 0x16,
    /// <summary>
    /// Size of a native integer.
    /// </summary>
    I = 0x18,
    /// <summary>
    /// Size of an unsigned native integer.
    /// </summary>
    U = 0x19,
    /// <summary>
    /// A pointer to a function.
    /// </summary>
    FNPTR = 0x1B,
    /// <summary>
    /// A System.Object type.
    /// </summary>
    OBJECT = 0x1C,
    /// <summary>
    /// A single-dimensional, zero lower-bound array type modifier.
    /// </summary>
    SZARRAY = 0x1D,
    /// <summary>
    /// A method variable type modifier.
    /// </summary>
    MVAR = 0x1e,
    /// <summary>
    /// A C language required modifier.
    /// </summary>
    CMOD_REQD = 0x1F,
    /// <summary>
    /// A C language required modifier.
    /// </summary>
    CMOD_OPT = 0x20,
    /// <summary>
    /// Used internally.
    /// </summary>
    INTERNAL = 0x21,
    /// <summary>
    /// An invalid type.
    /// </summary>
    MAX = 0x22,
    /// <summary>
    /// Used internally.
    /// </summary>
    MODIFIER = 0x40,
    /// <summary>
    /// A type modifier that is a sentinel for a list of a variable number of parameters.
    /// </summary>
    SENTINEL = 0x01 | 0x40,
    /// <summary>
    /// Used internally.
    /// </summary>
    PINNED = 0x05 | MODIFIER,
    SYSTEM = 0x100,
    CALLINGCONVENTION = 0x01 | SYSTEM,
    CALLCONVCDECL = 0x02 | SYSTEM,
    CALLCONVSTDCALL = 0x03 | SYSTEM,
    CALLCONVTHISCALL = 0x04 | SYSTEM,
    CALLCONVFASTCALL = 0x05 | SYSTEM,
    NULL = 0x999
}

public static class KnownTypeExtensions
{
    public static KnownType ToKnownType(this PrimitiveTypeCode primitiveTypeCode) => primitiveTypeCode switch
    {
        PrimitiveTypeCode.Boolean => KnownType.BOOLEAN,
        PrimitiveTypeCode.Char => KnownType.CHAR,
        PrimitiveTypeCode.SByte => KnownType.I1,
        PrimitiveTypeCode.Byte => KnownType.U1,
        PrimitiveTypeCode.Int16 => KnownType.I2,
        PrimitiveTypeCode.UInt16 => KnownType.U2,
        PrimitiveTypeCode.Int32 => KnownType.I4,
        PrimitiveTypeCode.UInt32 => KnownType.U4,
        PrimitiveTypeCode.Int64 => KnownType.I8,
        PrimitiveTypeCode.UInt64 => KnownType.U8,
        PrimitiveTypeCode.Single => KnownType.R4,
        PrimitiveTypeCode.Double => KnownType.R8,
        PrimitiveTypeCode.String => KnownType.STRING,
        PrimitiveTypeCode.IntPtr => KnownType.PTR,
        PrimitiveTypeCode.UIntPtr => KnownType.UPTR,
        PrimitiveTypeCode.Object => KnownType.OBJECT,
        PrimitiveTypeCode.Void => KnownType.VOID,
        PrimitiveTypeCode.TypedReference => KnownType.TYPEDBYREF,
        _ => throw new ArgumentException($"Unknown primitive type code: {primitiveTypeCode}")
    };
    public static PrimitiveTypeCode ToPrimitiveTypeCode(this KnownType knownType) => knownType switch
    {
        KnownType.BOOLEAN => PrimitiveTypeCode.Boolean,
        KnownType.CHAR => PrimitiveTypeCode.Char,
        KnownType.I1 => PrimitiveTypeCode.SByte,
        KnownType.U1 => PrimitiveTypeCode.Byte,
        KnownType.I2 => PrimitiveTypeCode.Int16,
        KnownType.U2 => PrimitiveTypeCode.UInt16,
        KnownType.I4 => PrimitiveTypeCode.Int32,
        KnownType.U4 => PrimitiveTypeCode.UInt32,
        KnownType.I8 => PrimitiveTypeCode.Int64,
        KnownType.U8 => PrimitiveTypeCode.UInt64,
        KnownType.R4 => PrimitiveTypeCode.Single,
        KnownType.R8 => PrimitiveTypeCode.Double,
        KnownType.STRING => PrimitiveTypeCode.String,
        KnownType.PTR => PrimitiveTypeCode.IntPtr,
        KnownType.UPTR => PrimitiveTypeCode.UIntPtr,
        KnownType.OBJECT => PrimitiveTypeCode.Object,
        KnownType.VOID => PrimitiveTypeCode.Void,
        KnownType.TYPEDBYREF => PrimitiveTypeCode.TypedReference,
        _ => throw new ArgumentException($"Unknown primitive type code: {knownType}")
    };
    public static KnownType ToKnownType(this ConstantTypeCode constantTypeCode) => constantTypeCode switch
    {
        ConstantTypeCode.Boolean => KnownType.BOOLEAN,
        ConstantTypeCode.Char => KnownType.CHAR,
        ConstantTypeCode.SByte => KnownType.I1,
        ConstantTypeCode.Byte => KnownType.U1,
        ConstantTypeCode.Int16 => KnownType.I2,
        ConstantTypeCode.UInt16 => KnownType.U2,
        ConstantTypeCode.Int32 => KnownType.I4,
        ConstantTypeCode.UInt32 => KnownType.U4,
        ConstantTypeCode.Int64 => KnownType.I8,
        ConstantTypeCode.UInt64 => KnownType.U8,
        ConstantTypeCode.Single => KnownType.R4,
        ConstantTypeCode.Double => KnownType.R8,
        ConstantTypeCode.String => KnownType.STRING,
        ConstantTypeCode.NullReference => KnownType.NULL,
        ConstantTypeCode.Invalid => KnownType.UNKNOWN,
        _ => throw new ArgumentException($"Unknown constant type code: {constantTypeCode}")
    };
}

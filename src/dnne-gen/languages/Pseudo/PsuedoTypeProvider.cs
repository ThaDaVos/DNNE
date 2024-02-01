using System;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using DNNE.Assembly;

namespace DNNE.Languages.Pseudo;

public class PseudoTypeProvider : AbstractSignatureTypeProvider<Assembly.Old.UnusedGenericContext>
{
    public override string GetArrayType(KnownType knownType, string elementType, ArrayShape shape)
    {
        return Enum.GetName(knownType) ?? "UNKNOWN";
    }

    public override string GetByReferenceType(KnownType knownType, string elementType)
    {
        return Enum.GetName(knownType) ?? "UNKNOWN";
    }

    public override string GetFunctionPointerType(MethodSignature<KnownType> knownSignature, MethodSignature<string> signature)
    {
        return Enum.GetName(knownSignature.ReturnType) ?? "UNKNOWN";
    }

    public override string GetGenericInstantiation(KnownType knownGenericType, string genericType, ImmutableArray<KnownType> knownTypeArguments, ImmutableArray<string> typeArguments)
    {
        var knownArgs = string.Join(", ", System.Linq.ImmutableArrayExtensions.Select(knownTypeArguments, x => Enum.GetName(x)));
        var args = string.Join(", ", typeArguments);
        return Enum.GetName(knownGenericType) ?? "UNKNOWN";
    }

    public override string GetModifiedType(KnownType knownModifier, string modifier, KnownType knownUnmodifiedType, string unmodifiedType, bool isRequired)
    {
        return Enum.GetName(knownModifier) ?? "UNKNOWN";
    }

    public override string GetPinnedType(KnownType knownType, string elementType)
    {
        return Enum.GetName(knownType) ?? "UNKNOWN";
    }

    public override string GetPointerType(KnownType knownType, string elementType)
    {
        return Enum.GetName(knownType) ?? "UNKNOWN";
    }

    public override string GetPrimitiveType(KnownType knownType, PrimitiveTypeCode typeCode)
    {
        return Enum.GetName(knownType) ?? "UNKNOWN";
    }

    public override string GetSystemType(KnownType knownType)
    {
        throw new NotImplementedException();
    }

    public override string GetSZArrayType(KnownType knownType, string elementType)
    {
        return Enum.GetName(knownType) ?? "UNKNOWN";
    }

    public override string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, KnownType knownRawTypeKind, byte rawTypeKind)
    {
        return Enum.GetName(knownRawTypeKind) ?? "UNKNOWN";
    }

    public override string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, KnownType knownRawTypeKind, byte rawTypeKind)
    {
        return Enum.GetName(knownRawTypeKind) ?? "UNKNOWN";
    }

    public override string GetTypeFromSerializedName(string typeName, string assemblySimpleName, KnownType knownType)
    {
        return $"{typeName} | {assemblySimpleName} | {Enum.GetName(knownType) ?? "UNKNOWN"}";
    }

    public override string GetTypeFromSpecification(MetadataReader reader, Assembly.Old.UnusedGenericContext? genericContext, TypeSpecificationHandle handle, KnownType knownRawTypeKind, byte rawTypeKind)
    {
        return Enum.GetName(knownRawTypeKind) ?? "UNKNOWN";
    }
}

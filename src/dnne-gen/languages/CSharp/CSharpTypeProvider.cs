using System;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using DNNE.Assembly;

namespace DNNE.Languages.CSharp;

public class CSharpTypeProvider : AbstractSignatureTypeProvider<GenericParametersContext>
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
        string? knownArgs = string.Join(", ", System.Linq.ImmutableArrayExtensions.Select(knownTypeArguments, x => Enum.GetName(x)));
        string? args = string.Join(", ", typeArguments);
        return Enum.GetName(knownGenericType) ?? "UNKNOWN";
    }

    public override string GetGenericMethodParameter(Assembly.GenericParameter parameter, GenericParametersContext? genericContext, int index)
    {
        return $"#{parameter.Name}";
    }

    public override string GetGenericTypeParameter(Assembly.GenericParameter parameter, GenericParametersContext? genericContext, int index)
    {
        throw new NotImplementedException();
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

    public override string GetTypeFromSpecification(MetadataReader reader, GenericParametersContext? genericContext, TypeSpecificationHandle handle, KnownType knownRawTypeKind, byte rawTypeKind)
    {
        return Enum.GetName(knownRawTypeKind) ?? "UNKNOWN";
    }
}

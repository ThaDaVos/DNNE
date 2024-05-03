using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;

namespace DNNE.Assembly;

public abstract class AbstractSignatureTypeProvider<TGenericContext> : ISignatureTypeProvider<string, TGenericContext?>, ICustomAttributeTypeProvider<string>
    where TGenericContext : GenericParametersContext
{
    #region Shared functions for Handling types
    private KnownType GetKnownTypeFromString(string elementType)
    {
        if (Enum.TryParse(elementType, out KnownType knownType))
        {
            return knownType;
        }

        if (elementType.Contains("NOT_IMPLEMENTED"))
        {
            return KnownType.UNKNOWN;
        }

        if (elementType.StartsWith("#"))
        {
            return KnownType.GENERICINST;
        }

        if (elementType.Contains("GENERIC_TYPE_PARAMETER"))
        {
            return KnownType.GENERICINST;
        }

        if (elementType.Contains("GENERIC_METHOD_PARAMETER"))
        {
            return KnownType.GENERICINST;
        }

        if (elementType.Contains("SYSTEM"))
        {
            return KnownType.SYSTEM;
        }

        throw new NotImplementedException();
    }

    #endregion

    #region Implementation of `ISignatureTypeProvider<string, TGenericContext>`
    public string GetArrayType(string elementType, ArrayShape shape) => GetArrayType(
        GetKnownTypeFromString(elementType),
        elementType,
        shape
    );
    public abstract string GetArrayType(KnownType knownType, string elementType, ArrayShape shape);

    public string GetByReferenceType(string elementType) => GetByReferenceType(
        GetKnownTypeFromString(elementType),
        elementType
    );
    public abstract string GetByReferenceType(KnownType knownType, string elementType);

    public string GetFunctionPointerType(MethodSignature<string> signature)
    {
        MethodSignature<KnownType> knownSignature = new(
            signature.Header,
            GetKnownTypeFromString(signature.ReturnType),
            signature.RequiredParameterCount,
            signature.GenericParameterCount,
            signature.ParameterTypes.Select(GetKnownTypeFromString).ToImmutableArray()
        );

        return GetFunctionPointerType(knownSignature, signature);
    }
    public abstract string GetFunctionPointerType(MethodSignature<KnownType> knownSignature, MethodSignature<string> signature);

    public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) => GetGenericInstantiation(
            GetKnownTypeFromString(genericType),
            genericType,
            typeArguments.Select(GetKnownTypeFromString).ToImmutableArray(),
            typeArguments
        );
    public abstract string GetGenericInstantiation(KnownType knownGenericType, string genericType, ImmutableArray<KnownType> knownTypeArguments, ImmutableArray<string> typeArguments);

    public string GetGenericMethodParameter(TGenericContext? genericContext, int index)
    {
        GenericParameter? parameter = genericContext?.GetGenericParameterByIndex(index);

        if (parameter.HasValue == false)
        {
            return $"#T{index}";
        }

        return GetGenericMethodParameter(parameter.Value, genericContext, index);
    }
    public abstract string GetGenericMethodParameter(GenericParameter parameter, TGenericContext? genericContext, int index);

    public string GetGenericTypeParameter(TGenericContext? genericContext, int index)
    {
        GenericParameter? parameter = genericContext?.GetGenericParameterByIndex(index);

        if (parameter.HasValue == false)
        {
            return $"#T{index}";
        }

        return GetGenericTypeParameter(parameter.Value, genericContext, index);
    }
    public abstract string GetGenericTypeParameter(GenericParameter parameter, TGenericContext? genericContext, int index);

    public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => GetModifiedType(
            GetKnownTypeFromString(modifier),
            modifier,
            GetKnownTypeFromString(unmodifiedType),
            unmodifiedType,
            isRequired
        );
    public abstract string GetModifiedType(KnownType knownModifier, string modifier, KnownType knownUnmodifiedType, string unmodifiedType, bool isRequired);

    public string GetPinnedType(string elementType) => GetPinnedType(
        GetKnownTypeFromString(elementType),
        elementType
    );
    public abstract string GetPinnedType(KnownType knownType, string elementType);

    public string GetPointerType(string elementType) => GetPointerType(
        GetKnownTypeFromString(elementType),
        elementType
    );
    public abstract string GetPointerType(KnownType knownType, string elementType);

    public string GetPrimitiveType(PrimitiveTypeCode typeCode) => GetPrimitiveType(typeCode.ToKnownType(), typeCode);
    public abstract string GetPrimitiveType(KnownType knownType, PrimitiveTypeCode typeCode);

    public string GetSZArrayType(string elementType) => GetSZArrayType(
        GetKnownTypeFromString(elementType),
        elementType
    );
    public abstract string GetSZArrayType(KnownType knownType, string elementType);

    public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => GetTypeFromDefinition(
            reader,
            handle,
            (KnownType)Enum.ToObject(typeof(KnownType), rawTypeKind),
            rawTypeKind
        );
    public abstract string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, KnownType knownRawTypeKind, byte rawTypeKind);

    public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => GetTypeFromReference(
            reader,
            handle,
            (KnownType)Enum.ToObject(typeof(KnownType), rawTypeKind),
            rawTypeKind
        );
    public abstract string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, KnownType knownRawTypeKind, byte rawTypeKind);

    public string GetTypeFromSpecification(MetadataReader reader, TGenericContext? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) => GetTypeFromSpecification(
            reader,
            genericContext,
            handle,
            (KnownType)Enum.ToObject(typeof(KnownType), rawTypeKind),
            rawTypeKind
        );
    public abstract string GetTypeFromSpecification(MetadataReader reader, TGenericContext? genericContext, TypeSpecificationHandle handle, KnownType knownRawTypeKind, byte rawTypeKind);
    #endregion

    #region Implementation of `ICustomAttributeTypeProvider<string>`
    public string GetSystemType() => GetSystemType(KnownType.SYSTEM);
    public abstract string GetSystemType(KnownType knownType);

    public string GetTypeFromSerializedName(string name)
    {
        int typeAssemblySeparator = name.IndexOf(',');
        string typeName = name[..typeAssemblySeparator];
        string assemblyName = name[(typeAssemblySeparator + 1)..];
        string assemblySimpleName = assemblyName;
        int simpleNameEnd = assemblySimpleName.IndexOf(',');
        if (simpleNameEnd != -1)
        {
            assemblySimpleName = assemblySimpleName[..simpleNameEnd];
        }

        return GetTypeFromSerializedName(
            typeName,
            assemblySimpleName.TrimStart(),
            (typeName, assemblySimpleName.TrimStart()) switch
            {
                ("System.Runtime.InteropServices.CallingConvention", "System.Runtime.InteropServices") => KnownType.CALLINGCONVENTION,
                ("System.Runtime.CompilerServices.CallConvCdecl", "System.Runtime") => KnownType.CALLCONVCDECL,
                ("System.Runtime.CompilerServices.CallConvStdcall", "System.Runtime") => KnownType.CALLCONVSTDCALL,
                ("System.Runtime.CompilerServices.CallConvThiscall", "System.Runtime") => KnownType.CALLCONVTHISCALL,
                ("System.Runtime.CompilerServices.CallConvFastcall", "System.Runtime") => KnownType.CALLCONVFASTCALL,
                _ => KnownType.UNKNOWN
            }
        );
    }
    public abstract string GetTypeFromSerializedName(string typeName, string assemblySimpleName, KnownType knownType);

    public PrimitiveTypeCode GetUnderlyingEnumType(string type) => GetKnownTypeFromString(type).ToPrimitiveTypeCode();

    public bool IsSystemType(string type) => GetKnownTypeFromString(type) == KnownType.SYSTEM;
    #endregion
}

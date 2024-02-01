using System;
using System.Reflection.Metadata;

namespace DNNE;

public class UsedCustomAttribute<TType>
{
    protected readonly MetadataReader metadataReader;
    protected readonly CustomAttribute customAttribute;
    protected readonly ICustomAttributeTypeProvider<TType> customAttributeTypeProvider;
    private StringHandle? nameStringHandle;
    private StringHandle? namespaceStringHandle;
    private string? name;
    internal string Name => name ??= GetName();
    private string? @namespace;
    internal string Namespace => @namespace ??= GetNamespace();
    public UsedCustomAttribute(MetadataReader metadataReader, CustomAttribute customAttribute, ICustomAttributeTypeProvider<TType> customAttributeTypeProvider)
    {
        this.customAttribute = customAttribute;
        this.metadataReader = metadataReader;
        this.customAttributeTypeProvider = customAttributeTypeProvider;
    }

    private (StringHandle nameStringHandle, StringHandle namespaceStringHandle) DecodeConstructor()
    {
        StringHandle nameStringHandle;
        StringHandle namespaceStringHandle;
        switch (customAttribute.Constructor.Kind)
        {
            case HandleKind.MemberReference:
                MemberReference refConstructor = metadataReader.GetMemberReference((MemberReferenceHandle)customAttribute.Constructor);
                TypeReference refType = metadataReader.GetTypeReference((TypeReferenceHandle)refConstructor.Parent);
                namespaceStringHandle = refType.Namespace;
                nameStringHandle = refType.Name;
                break;

            case HandleKind.MethodDefinition:
                MethodDefinition defConstructor = metadataReader.GetMethodDefinition((MethodDefinitionHandle)customAttribute.Constructor);
                TypeDefinition defType = metadataReader.GetTypeDefinition(defConstructor.GetDeclaringType());
                namespaceStringHandle = defType.Namespace;
                nameStringHandle = defType.Name;
                break;

            default:
                throw new ArgumentException("Unexpected constructor kind.");
        }

        return (nameStringHandle, namespaceStringHandle);
    }

    private string GetName()
    {
        if (nameStringHandle == null)
        {
            (nameStringHandle, namespaceStringHandle) = DecodeConstructor();
        }

        return nameStringHandle.HasValue ? metadataReader.GetString(nameStringHandle.Value) : "UNKNOWN";
    }
    private string GetNamespace()
    {
        if (namespaceStringHandle == null)
        {
            (nameStringHandle, namespaceStringHandle) = DecodeConstructor();
        }

        return namespaceStringHandle.HasValue ? metadataReader.GetString(namespaceStringHandle.Value) : "UNKNOWN";
    }
}

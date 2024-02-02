using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using DNNE.Assembly.Old;

namespace DNNE.Assembly;

public class UsedAttribute<TType>
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
    public UsedAttribute(MetadataReader metadataReader, CustomAttribute customAttribute, ICustomAttributeTypeProvider<TType> customAttributeTypeProvider)
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

    private Dictionary<string, UsedAttributeArgument<TType>> GetAttributeArgs()
    {
        string[] constructorArgumentNames = GetArgumentNamesFromCustomAttribute(metadataReader, customAttribute);

        Dictionary<string, UsedAttributeArgument<TType>>? arguments = new();

        CustomAttributeValue<TType> data = customAttribute.DecodeValue(customAttributeTypeProvider);

        int count = 0;
        foreach (CustomAttributeTypedArgument<TType> item in data.FixedArguments)
        {
            arguments.Add(constructorArgumentNames[count], new UsedAttributeArgument<TType>()
            {
                Type = item.Type,
                Value = item.Value,
            });

            count++;
        }

        foreach (CustomAttributeNamedArgument<TType> item in data.NamedArguments)
        {
            arguments.Add(item.Name, new UsedAttributeArgument<TType>()
            {
                Type = item.Type,
                Value = item.Value,
            });
        }

        return arguments;
    }

    static string[] GetArgumentNamesFromCustomAttribute(MetadataReader reader, CustomAttribute attribute) => attribute.Constructor.Kind switch
    {
        HandleKind.MemberReference => GetArgumentNamesFromCustomAttributeWithConstructorReference(reader, attribute),
        HandleKind.MethodDefinition => GetArgumentNamesFromCustomAttributeWithConstructorDefinition(reader, attribute),
        _ => throw new ArgumentOutOfRangeException($"Unknown attribute constructor kind: {Enum.GetName(attribute.Constructor.Kind)}"),
    };

    private static string[] GetArgumentNamesFromCustomAttributeWithConstructorReference(MetadataReader reader, CustomAttribute attribute)
    {
        // MemberReference refConstructor = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);

        // ParameterHandleCollection parameters = refConstructor.GetParameters();
        // List<string> names = new(parameters.Count);
        // foreach (ParameterHandle param in parameters)
        // {
        //     names.Add(reader.GetString(reader.GetParameter(param).Name));
        // }

        // return names.ToArray();

        return [];
    }
    private static string[] GetArgumentNamesFromCustomAttributeWithConstructorDefinition(MetadataReader reader, CustomAttribute attribute)
    {
        MethodDefinition defConstructor = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);

        ParameterHandleCollection parameters = defConstructor.GetParameters();
        List<string> names = new(parameters.Count);
        foreach (ParameterHandle param in parameters)
        {
            names.Add(reader.GetString(reader.GetParameter(param).Name));
        }

        return names.ToArray();
    }
}

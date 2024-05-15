using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;
using DNNE.Languages.CSharp;

namespace DNNE.Assembly.Entities;

internal class ExportedAttribute : ExportedEntity<CustomAttributeHandle>, IExportedAttribute
{
    private CustomAttribute? customAttribute;
    private CustomAttribute CustomAttribute => customAttribute ??= metadataReader.GetCustomAttribute(entity);
    private StringHandle? nameStringHandle;
    private StringHandle? namespaceStringHandle;

    public ExportedAttribute(MetadataReader metadataReader, CustomAttributeHandle handle, IExportedEntity? parent = null) : base(metadataReader, handle, parent)
    {
    }

    private string? @namespace;
    public string Namespace => @namespace ??= GetNamespace();
    private (IEnumerable<FixedArgumentOfExportedAttribute> FixedArguments, IEnumerable<NamedArgumentOfExportedAttribute> NamedArguments)? arguments;
    public IEnumerable<FixedArgumentOfExportedAttribute> FixedArguments => arguments?.FixedArguments ?? (arguments = GetArguments()).Value.FixedArguments;
    public IEnumerable<NamedArgumentOfExportedAttribute> NamedArguments => arguments?.NamedArguments ?? (arguments = GetArguments()).Value.NamedArguments;

    private (StringHandle nameStringHandle, StringHandle namespaceStringHandle) DecodeConstructor()
    {
        StringHandle nameStringHandle;
        StringHandle namespaceStringHandle;
        switch (CustomAttribute.Constructor.Kind)
        {
            case HandleKind.MemberReference:
                MemberReference refConstructor = metadataReader.GetMemberReference((MemberReferenceHandle)CustomAttribute.Constructor);
                TypeReference refType = metadataReader.GetTypeReference((TypeReferenceHandle)refConstructor.Parent);
                namespaceStringHandle = refType.Namespace;
                nameStringHandle = refType.Name;
                break;

            case HandleKind.MethodDefinition:
                MethodDefinition defConstructor = metadataReader.GetMethodDefinition((MethodDefinitionHandle)CustomAttribute.Constructor);
                TypeDefinition defType = metadataReader.GetTypeDefinition(defConstructor.GetDeclaringType());
                namespaceStringHandle = defType.Namespace;
                nameStringHandle = defType.Name;
                break;

            default:
                throw new ArgumentException("Unexpected constructor kind.");
        }

        return (nameStringHandle, namespaceStringHandle);
    }

    protected override string GetName()
    {
        if (nameStringHandle == null)
        {
            (nameStringHandle, namespaceStringHandle) = DecodeConstructor();
        }

        return nameStringHandle.HasValue ? metadataReader.GetString(nameStringHandle.Value) : "UNKNOWN";
    }
    protected string GetNamespace()
    {
        if (namespaceStringHandle == null)
        {
            (nameStringHandle, namespaceStringHandle) = DecodeConstructor();
        }

        return namespaceStringHandle.HasValue ? metadataReader.GetString(namespaceStringHandle.Value) : "UNKNOWN";
    }

    public (IEnumerable<FixedArgumentOfExportedAttribute> FixedArguments, IEnumerable<NamedArgumentOfExportedAttribute> NamedArguments) GetArguments(AbstractSignatureTypeProvider<GenericParametersContext>? provider = null)
    {
        provider ??= new CSharpTypeProvider();

        string[] constructorArgumentNames = GetArgumentNamesFromCustomAttribute(provider);

        // Use different TypeProvider for decoding the attribute value
        CustomAttributeValue<string> data = CustomAttribute.DecodeValue(provider);

        return (
            data.FixedArguments
                .Select(
                    (CustomAttributeTypedArgument<string> item, int index)
                        => new FixedArgumentOfExportedAttribute(metadataReader, item, constructorArgumentNames[index], this)
                ),
            data.NamedArguments
                .Select(
                    (CustomAttributeNamedArgument<string> item)
                        => new NamedArgumentOfExportedAttribute(metadataReader, item, this)
                )
        );
    }

    private string[] GetArgumentNamesFromCustomAttribute(AbstractSignatureTypeProvider<GenericParametersContext> provider)
    {
        if (CustomAttribute.Constructor.Kind == HandleKind.MemberReference)
        {
            throw new NotImplementedException("MemberReference constructor is not supported.");
        }

        MethodDefinition defConstructor = metadataReader.GetMethodDefinition((MethodDefinitionHandle)CustomAttribute.Constructor);

        ParameterHandleCollection parameters = defConstructor.GetParameters();
        List<string> names = new(parameters.Count);
        foreach (ParameterHandle param in parameters)
        {
            names.Add(metadataReader.GetString(metadataReader.GetParameter(param).Name));
        }

        return names.ToArray();
    }
}

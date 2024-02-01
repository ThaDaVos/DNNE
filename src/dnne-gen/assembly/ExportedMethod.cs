using System.Reflection.Metadata;
using DNNE.Assembly;

namespace DNNE.Assembly;

internal class ExportedMethod : ExportedEntity<MethodDefinition>
{
    public ExportedMethod(MetadataReader metadataReader, MethodDefinition definition) : base(metadataReader, definition)
    {
    }
    protected override string GetName() => metadataReader.GetString(definition.Name);
    protected override CustomAttributeHandleCollection GetCustomAttributeHandles() => definition.GetCustomAttributes();
    internal TProviderReturn GetReturnType<TProviderReturn>(ISignatureTypeProvider<TProviderReturn, Old.UnusedGenericContext?> typeProvider)
        => GetReturnType<TProviderReturn, Old.UnusedGenericContext?>(typeProvider);

    internal TProviderReturn GetReturnType<TProviderReturn, TGenericContext>(ISignatureTypeProvider<TProviderReturn, TGenericContext?> typeProvider) where TGenericContext : new()
    {
        MethodSignature<TProviderReturn> signature = definition.DecodeSignature(typeProvider, genericContext: new());

        return signature.ReturnType;
    }
}

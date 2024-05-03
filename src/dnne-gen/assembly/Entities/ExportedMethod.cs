using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;
using DNNE.Languages.CSharp;

namespace DNNE.Assembly.Entities;

internal class ExportedMethod : ExportedAttributedEntity<MethodDefinition>, IExportedMethod
{
    private string? returnType;
    public string ReturnType => returnType ??= GetReturnType();

    public ExportedMethod(MetadataReader metadataReader, MethodDefinition entity) : base(metadataReader, entity)
    {
    }
    protected override string GetName() => metadataReader.GetString(entity.Name);
    protected override CustomAttributeHandleCollection GetCustomAttributeHandles() => entity.GetCustomAttributes();

    public string GetReturnType(AbstractSignatureTypeProvider<GenericParametersContext>? provider = null)
    {
        MethodSignature<string> signature = entity.DecodeSignature(provider ?? new CSharpTypeProvider(), genericContext: null);

        return signature.ReturnType;
    }
}

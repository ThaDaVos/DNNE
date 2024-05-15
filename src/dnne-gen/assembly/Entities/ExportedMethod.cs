using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using DNNE.Assembly.Entities.Interfaces;
using DNNE.Languages.CSharp;

namespace DNNE.Assembly.Entities;

internal class ExportedMethod : ExportedAttributedEntity<MethodDefinition>, IExportedMethod
{
    private string? returnType;
    public string ReturnType => returnType ??= GetReturnType();
    private MethodSignature<string>? signature;

    private IEnumerable<ExportedMethodParameter>? parameters;
    public IEnumerable<ExportedMethodParameter> Parameters => parameters ??= GetParameters();

    public ExportedMethod(MetadataReader metadataReader, MethodDefinition entity, IExportedEntity? parent = null) : base(metadataReader, entity, parent)
    {
    }
    protected override string GetName() => metadataReader.GetString(entity.Name);
    protected override CustomAttributeHandleCollection GetCustomAttributeHandles() => entity.GetCustomAttributes();

    public string GetReturnType(AbstractSignatureTypeProvider<GenericParametersContext>? provider = null)
        => Parameters
            // Return type is always the first parameter as the enumerator is ordered by sequence number - but just in case we filter
            .FirstOrDefault(parameter => parameter.SequenceNumber == 0)?.Type
                ?? signature?.ReturnType
                ?? throw new InvalidOperationException("Return type not found");

    public IEnumerable<ExportedMethodParameter> GetParameters(AbstractSignatureTypeProvider<GenericParametersContext>? provider = null)
    {
        signature ??= entity.DecodeSignature(provider ?? new CSharpTypeProvider(), genericContext: null);

        return entity
            .GetParameters()
            .Select(
                handle =>
                {
                    Parameter parameter = metadataReader.GetParameter(handle);

                    return new ExportedMethodParameter(
                        metadataReader: metadataReader,
                        entity: parameter,
                        type: (parameter.SequenceNumber > 0 ? signature?.ParameterTypes[parameter.SequenceNumber - 1] : signature?.ReturnType)
                            ?? throw new InvalidOperationException("Parameter type not found in signature"),
                        parent: this
                    );
                }
            )
            .OrderBy(parameter => parameter.SequenceNumber);
    }
}

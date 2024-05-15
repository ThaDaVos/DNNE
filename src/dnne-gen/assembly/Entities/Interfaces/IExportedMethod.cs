using System.Collections.Generic;

namespace DNNE.Assembly.Entities.Interfaces;

public interface IExportedMethod : IExportedAttributedEntity
{
    internal string ReturnType { get; }
    internal string GetReturnType(AbstractSignatureTypeProvider<GenericParametersContext>? provider = null);
    internal IEnumerable<ExportedMethodParameter> Parameters { get; }
    internal IEnumerable<ExportedMethodParameter> GetParameters(AbstractSignatureTypeProvider<GenericParametersContext>? provider = null);
}

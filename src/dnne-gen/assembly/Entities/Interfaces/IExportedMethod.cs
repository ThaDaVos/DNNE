namespace DNNE.Assembly.Entities.Interfaces;

public interface IExportedMethod : IExportedAttributedEntity
{
    internal string ReturnType { get; }
    internal string GetReturnType(AbstractSignatureTypeProvider<GenericParametersContext>? provider = null);
}

namespace DNNE.Assembly.Entities.Interfaces;

public interface IExportedConstantValuedEntity : IExportedValuedEntity
{
    internal KnownType KnownType { get; }
}

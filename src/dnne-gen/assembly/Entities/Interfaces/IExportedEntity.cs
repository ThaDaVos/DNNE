namespace DNNE.Assembly.Entities.Interfaces;

public interface IExportedEntity
{
    internal IExportedEntity? Parent { get; }
    internal string Name { get; }
}

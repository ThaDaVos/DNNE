using System.Collections.Generic;

namespace DNNE.Assembly.Entities.Interfaces;

public interface IExportedAttribute : IExportedEntity 
{
    internal string Namespace { get; }
    internal IEnumerable<FixedArgumentOfExportedAttribute> FixedArguments { get; }
    internal IEnumerable<NamedArgumentOfExportedAttribute> NamedArguments { get; }
}

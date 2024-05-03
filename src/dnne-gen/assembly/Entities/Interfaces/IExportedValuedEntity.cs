using System;

namespace DNNE.Assembly.Entities.Interfaces;

public interface IExportedValuedEntity : IExportedEntity
{
    internal dynamic? Value { get; }
    internal Type Type { get; }
    internal bool IsNil { get; }
}

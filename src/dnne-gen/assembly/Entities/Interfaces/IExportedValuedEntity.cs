using System;

namespace DNNE.Assembly.Entities.Interfaces;

public interface IExportedValuedEntity : IExportedEntity
{
    internal dynamic? Value { get; }
    internal string Type { get; }
    internal bool IsNil { get; }
}
